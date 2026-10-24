using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;

namespace WebStore.Tests;

public class SqlServerIntegrationTests
{
    [Fact]
    [Trait("Category", "SqlServer")]
    public void MigrationPersistsRelationsTphAndServiceOperationsOnRealSqlServer()
    {
        using var h = new TestHost(useSql: true);
        Assert.NotEmpty(h.Db.Database.GetAppliedMigrations());
        var orders = h.Get<IOrderService>();
        var first = orders.AddOrUpdateOrder(h.Order());
        var second = orders.AddOrUpdateOrder(h.Order());
        var invoiceService = h.Get<IInvoiceService>();
        var invoice = invoiceService.AddOrUpdateInvoice(h.Invoice(first.Id, second.Id));
        var address = h.Address(); address.MakeDefaultBilling = true; address.MakeDefaultShipping = true;
        var savedAddress = h.Get<IAddressService>().AddOrUpdateAddress(address);
        var storeAddress = h.Address(); storeAddress.CustomerId = null;
        storeAddress.StationaryStoreId = h.StoreId; storeAddress.AddressType = AddressType.Store;
        h.Get<IAddressService>().AddOrUpdateAddress(storeAddress);
        h.Get<IAddressService>().AddOrUpdateAddress(storeAddress);
        h.Db.StationaryStoreEmployees.Add(new StationaryStoreEmployee
        {
            FirstName = "Ewa", LastName = "Nowak", Position = "Sprzedawca", StationaryStoreId = h.StoreId
        });
        h.Db.SaveChanges();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(h.Db.Database.GetConnectionString()).Options;
        using (var fresh = new ApplicationDbContext(options))
        {
            Assert.Equal(4, fresh.Users.Count());
            Assert.Equal(2, fresh.Customers.Count()); Assert.Single(fresh.Suppliers); Assert.Single(fresh.StationaryStoreEmployees);
            var customer = fresh.Customers.Include(x => x.BillingAddress).Include(x => x.ShippingAddress)
                .Include(x => x.Addresses).AsSplitQuery().Single(x => x.Id == h.CustomerId);
            Assert.Equal(savedAddress.Id, customer.BillingAddress!.Id);
            Assert.Equal(savedAddress.Id, customer.ShippingAddress!.Id);
            Assert.Single(customer.Addresses);
            var persistedInvoice = fresh.Invoices.Include(x => x.Orders).Single();
            Assert.Equal(2, persistedInvoice.Orders.Count); Assert.Equal(4000m, persistedInvoice.TotalAmount);
            Assert.Equal(2, fresh.OrderProducts.Count());
            Assert.Equal(2, fresh.Addresses.Count(x => x.StationaryStoreId == h.StoreId));
            Assert.Equal(7, fresh.ProductStocks.Sum(x => x.Quantity));
            Assert.All(fresh.Products, x => Assert.Equal(h.CategoryId, x.CategoryId));
        }
        // Zmiana powiązań faktury pozostawia spójny stan także w relacyjnej bazie.
        var update = h.Invoice(second.Id); update.Id = invoice.Id;
        invoiceService.AddOrUpdateInvoice(update);
        h.Db.ChangeTracker.Clear();
        Assert.Null(h.Db.Orders.Single(x => x.Id == first.Id).InvoiceId);
        Assert.Equal(invoice.Id, h.Db.Orders.Single(x => x.Id == second.Id).InvoiceId);
        Assert.Throws<InvalidOperationException>(() => orders.AddOrUpdateOrder(h.Order(second.Id)));
        Assert.Equal(2, h.Db.Orders.Count());
        // Ograniczenia są egzekwowane także przy pominięciu usług.
        h.Db.OrderProducts.Add(new OrderProduct { OrderId = first.Id, ProductId = h.OtherProductId, Quantity = 0, UnitPrice = 1 });
        Assert.Throws<DbUpdateException>(() => h.Db.SaveChanges()); h.Db.ChangeTracker.Clear();
        h.Db.ProductStocks.Add(new ProductStock { ProductId = int.MaxValue, Quantity = 1 });
        Assert.Throws<DbUpdateException>(() => h.Db.SaveChanges()); h.Db.ChangeTracker.Clear();
        h.Db.Addresses.Add(new Address { Country = "PL", PostalCode = "1", City = "A", Street = "B", BuildingNumber = "1", AddressType = AddressType.Billing });
        Assert.Throws<DbUpdateException>(() => h.Db.SaveChanges()); h.Db.ChangeTracker.Clear();
        h.Db.Invoices.Add(new Invoice { Number = "fv/2026/1", IssueDate = new DateTime(2026, 10, 10), CustomerId = h.CustomerId });
        Assert.Throws<DbUpdateException>(() => h.Db.SaveChanges()); h.Db.ChangeTracker.Clear();
    }
}
