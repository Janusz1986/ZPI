using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;

namespace WebStore.Tests;

public class ReviewRegressionTests
{
    [Fact]
    [Trait("Category", "SqlServer")]
    public void RejectsOrderUpdateAfterAnotherScopeIssuesInvoice()
    {
        using var h = new TestHost(useSql: true);
        var orders = h.Get<IOrderService>();
        var order = orders.AddOrUpdateOrder(h.Order());
        int invoiceId;
        using (var other = h.CreateScope())
            invoiceId = other.ServiceProvider.GetRequiredService<IInvoiceService>().AddOrUpdateInvoice(h.Invoice(order.Id)).Id;
        var update = h.Order(order.Id); update.Items[0].Quantity = 3;
        Assert.Throws<InvalidOperationException>(() => orders.AddOrUpdateOrder(update));
        using var fresh = h.CreateScope();
        var db = fresh.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(invoiceId, db.Orders.Single().InvoiceId);
        Assert.Equal(2000m, db.Orders.Single().TotalAmount);
        Assert.Equal(2000m, db.Invoices.Single().TotalAmount);
        Assert.Equal(2, db.OrderProducts.Single().Quantity);
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public void InvoiceUsesAmountUpdatedInAnotherScope()
    {
        using var h = new TestHost(useSql: true);
        var order = h.Get<IOrderService>().AddOrUpdateOrder(h.Order());
        using (var other = h.CreateScope())
        {
            var update = h.Order(order.Id); update.Items[0].Quantity = 3;
            other.ServiceProvider.GetRequiredService<IOrderService>().AddOrUpdateOrder(update);
        }
        var invoice = h.Get<IInvoiceService>().AddOrUpdateInvoice(h.Invoice(order.Id));
        Assert.Equal(3000m, invoice.TotalAmount);
        using var fresh = h.CreateScope();
        var db = fresh.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(db.Orders.Single().TotalAmount, db.Invoices.Single().TotalAmount);
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public void CannotMoveOrderToAnotherInvoiceUsingStaleContext()
    {
        using var h = new TestHost(useSql: true);
        var order = h.Get<IOrderService>().AddOrUpdateOrder(h.Order());
        int invoiceId;
        using (var other = h.CreateScope())
            invoiceId = other.ServiceProvider.GetRequiredService<IInvoiceService>().AddOrUpdateInvoice(h.Invoice(order.Id)).Id;
        var second = h.Invoice(order.Id); second.Number = "FV/2026/2";
        Assert.Throws<InvalidOperationException>(() => h.Get<IInvoiceService>().AddOrUpdateInvoice(second));
        using var fresh = h.CreateScope();
        var db = fresh.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Single(db.Invoices);
        Assert.Equal(invoiceId, db.Orders.Single().InvoiceId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("Category", "SqlServer")]
    public void AddressValidationUsesCurrentDefaultSelection(bool selectOriginal)
    {
        using var h = new TestHost(useSql: true);
        var addresses = h.Get<IAddressService>();
        var vm = h.Address(); vm.MakeDefaultBilling = !selectOriginal;
        var original = addresses.AddOrUpdateAddress(vm);
        int selectedId;
        using (var other = h.CreateScope())
        {
            var selected = h.Address(selectOriginal ? original.Id : null); selected.MakeDefaultBilling = true;
            selectedId = other.ServiceProvider.GetRequiredService<IAddressService>().AddOrUpdateAddress(selected).Id;
        }
        var update = h.Address(original.Id); update.AddressType = AddressType.Shipping;
        if (selectOriginal) Assert.Throws<ArgumentException>(() => addresses.AddOrUpdateAddress(update));
        else Assert.Equal(AddressType.Shipping, addresses.AddOrUpdateAddress(update).AddressType);
        using var fresh = h.CreateScope();
        var db = fresh.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(selectedId, db.Customers.Single(x => x.Id == h.CustomerId).BillingAddressId);
        Assert.Equal(selectOriginal ? AddressType.BillingAndShipping : AddressType.Shipping,
            db.Addresses.Single(x => x.Id == original.Id).AddressType);
    }

    [Fact]
    public void PendingCallerChangesAreNotDiscardedByService()
    {
        using var h = new TestHost();
        var product = h.Db.Products.Single(x => x.Id == h.ProductId); product.Name = "Zmiana Lidera";
        Assert.Throws<InvalidOperationException>(() => h.Get<IOrderService>().AddOrUpdateOrder(h.Order()));
        Assert.True(h.Db.ChangeTracker.HasChanges());
        Assert.Equal("Zmiana Lidera", product.Name);
        h.Db.SaveChanges(); h.Db.ChangeTracker.Clear();
        Assert.Equal("Zmiana Lidera", h.Db.Products.Single(x => x.Id == h.ProductId).Name);
        Assert.Empty(h.Db.Orders);
    }

    public static IEnumerable<object?[]> InvalidEmployeeFields()
    {
        foreach (var field in new[] { "FirstName", "LastName", "Position" })
            foreach (var value in new string?[] { null, "", " \t" })
                yield return [field, value];
    }

    [Theory]
    [MemberData(nameof(InvalidEmployeeFields))]
    public async Task IdentityRejectsEmptyEmployeeFieldsOnCreateAndUpdate(string field, string? value)
    {
        using var h = new TestHost();
        var manager = h.Get<UserManager<User>>();
        var invalid = Employee(h.StoreId, "invalid"); SetField(invalid, field, value);
        var create = await manager.CreateAsync(invalid);
        Assert.False(create.Succeeded);
        Assert.Contains(create.Errors, x => x.Code == $"Required{field}");
        Assert.Empty(h.Db.StationaryStoreEmployees);
        var valid = Employee(h.StoreId, "valid");
        Assert.True((await manager.CreateAsync(valid)).Succeeded);
        SetField(valid, field, value);
        var update = await manager.UpdateAsync(valid);
        Assert.False(update.Succeeded);
        Assert.Contains(update.Errors, x => x.Code == $"Required{field}");
        using var fresh = h.CreateScope();
        var persisted = fresh.ServiceProvider.GetRequiredService<ApplicationDbContext>().StationaryStoreEmployees.Single();
        Assert.Equal("Ewa", persisted.FirstName); Assert.Equal("Nowak", persisted.LastName); Assert.Equal("Sprzedawca", persisted.Position);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task DirectContextSaveAlsoRejectsInvalidEmployee(bool asynchronous)
    {
        using var h = new TestHost();
        var employee = Employee(h.StoreId, "direct"); employee.Position = " \t";
        h.Db.StationaryStoreEmployees.Add(employee);
        if (asynchronous) await Assert.ThrowsAsync<ValidationException>(() => h.Db.SaveChangesAsync());
        else Assert.Throws<ValidationException>(() => h.Db.SaveChanges());
        h.Db.ChangeTracker.Clear();
        Assert.Empty(h.Db.StationaryStoreEmployees);
    }

    [Fact]
    [Trait("Category", "SqlServer")]
    public async Task EmployeeValidationAndValidUpdatesPersistOnSqlServer()
    {
        using var h = new TestHost(useSql: true);
        var manager = h.Get<UserManager<User>>();
        var invalid = Employee(h.StoreId, "invalid-sql"); invalid.FirstName = " "; invalid.LastName = ""; invalid.Position = "";
        Assert.False((await manager.CreateAsync(invalid)).Succeeded);
        Assert.Empty(h.Db.StationaryStoreEmployees);
        var valid = Employee(h.StoreId, "valid-sql");
        Assert.True((await manager.CreateAsync(valid)).Succeeded);
        valid.Position = "Kierownik";
        Assert.True((await manager.UpdateAsync(valid)).Succeeded);
        valid.LastName = " "; Assert.False((await manager.UpdateAsync(valid)).Succeeded);
        using (var fresh = h.CreateScope())
        {
            var saved = fresh.ServiceProvider.GetRequiredService<ApplicationDbContext>().StationaryStoreEmployees.Single();
            Assert.Equal("Nowak", saved.LastName); Assert.Equal("Kierownik", saved.Position);
            Assert.Equal(h.StoreId, saved.StationaryStoreId);
        }
        h.Db.ChangeTracker.Clear();
        h.Db.StationaryStoreEmployees.Add(invalid);
        await Assert.ThrowsAsync<ValidationException>(() => h.Db.SaveChangesAsync());
        h.Db.ChangeTracker.Clear(); Assert.Single(h.Db.StationaryStoreEmployees);
    }

    private static StationaryStoreEmployee Employee(int storeId, string name) => new()
    {
        UserName = name, FirstName = "Ewa", LastName = "Nowak", Position = "Sprzedawca", StationaryStoreId = storeId
    };
    private static void SetField(StationaryStoreEmployee employee, string field, string? value)
    {
        switch (field)
        {
            case "FirstName": employee.FirstName = value!; break;
            case "LastName": employee.LastName = value!; break;
            case "Position": employee.Position = value!; break;
        }
    }
}
