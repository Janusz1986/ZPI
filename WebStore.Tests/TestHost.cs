using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Configuration;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Tests;

public sealed class TestHost : IDisposable
{
    private readonly ServiceProvider provider;
    private readonly IServiceScope scope;
    private readonly bool sql;
    public ApplicationDbContext Db { get; }
    public int CustomerId { get; }
    public int OtherCustomerId { get; }
    public int SupplierId { get; }
    public int CategoryId { get; }
    public int ProductId { get; }
    public int OtherProductId { get; }
    public int StoreId { get; }
    public T Get<T>() where T : notnull => scope.ServiceProvider.GetRequiredService<T>();
    public IServiceScope CreateScope() => provider.CreateScope();

    public TestHost(bool useSql = false)
    {
        sql = useSql;
        var services = new ServiceCollection();
        var name = "ZPI_WebStore_Test_" + Guid.NewGuid().ToString("N");
        services.AddDbContext<ApplicationDbContext>(o =>
        {
            if (useSql) o.UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database={name};Trusted_Connection=True;TrustServerCertificate=True");
            else o.UseInMemoryDatabase(name);
        });
        services.AddWebStoreServices();
        provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
        scope = provider.CreateScope();
        Db = Get<ApplicationDbContext>();
        if (sql) Db.Database.Migrate();
        else Db.Database.EnsureCreated();
        var customer = new Customer { FirstName = "Jan", LastName = "Klient", UserName = "jan", RegistrationDate = new DateTime(2026, 1, 1) };
        var other = new Customer { FirstName = "Anna", LastName = "Klient", UserName = "anna" };
        var supplier = new Supplier { FirstName = "Adam", LastName = "Bednarski", UserName = "supp1@eg.eg", Email = "supp1@eg.eg", RegistrationDate = new DateTime(2010, 1, 1) };
        var category = new Category { Name = "Computers", Tag = "#computer" };
        var store = new StationaryStore { Name = "Sklep centralny" };
        var product = new Product { Name = "Monitor Dell 32", Description = "Bardzo praktyczny monitor 32 cale", Price = 1000m, Weight = 20, ImageBytes = [255, 255, 255, 128], Category = category, Supplier = supplier };
        var mouse = new Product { Name = "Mysz Logitech", Description = "Precyzyjna mysz do pracy", Price = 500m, Weight = 0.5f, ImageBytes = [255, 255, 255, 112], Category = category, Supplier = supplier };
        Db.AddRange(customer, other, supplier, category, store, product, mouse);
        Db.SaveChanges();
        Db.AddRange(new ProductStock { ProductId = product.Id, Quantity = 3 }, new ProductStock { ProductId = product.Id, Quantity = 4 });
        Db.SaveChanges();
        CustomerId = customer.Id; OtherCustomerId = other.Id; SupplierId = supplier.Id;
        CategoryId = category.Id; ProductId = product.Id; OtherProductId = mouse.Id; StoreId = store.Id;
        Db.ChangeTracker.Clear();
    }

    public AddOrUpdateProductVm Product(int? id = null) => new()
    {
        Id = id, Name = "MacBook Pro", Description = "MacBook Pro 8GB RAM", Price = 6000m,
        Weight = 1.1f, ImageBytes = [255, 255, 255, 128], CategoryId = CategoryId, SupplierId = SupplierId
    };
    public AddOrUpdateOrderVm Order(int? id = null) => new()
    {
        Id = id, CustomerId = CustomerId, OrderDate = new DateTime(2026, 10, 1),
        DeliveryDate = new DateTime(2026, 10, 5), Items = [new() { ProductId = ProductId, Quantity = 2 }]
    };
    public AddOrUpdateAddressVm Address(int? id = null) => new()
    {
        Id = id, Country = "Polska", PostalCode = "00-001", City = "Warszawa", Street = "Długa",
        BuildingNumber = "1", CustomerId = CustomerId, AddressType = AddressType.BillingAndShipping
    };
    public AddOrUpdateInvoiceVm Invoice(params int[] ids) => new()
    {
        Number = "FV/2026/1", IssueDate = new DateTime(2026, 10, 10), OrderIds = ids.ToList()
    };
    public void Dispose()
    {
        // Wyłącznie izolowana baza utworzona przez ten obiekt, o losowej nazwie testowej.
        if (sql) Db.Database.EnsureDeleted();
        scope.Dispose(); provider.Dispose();
    }
}
