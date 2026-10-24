using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebStore.Model.DataModels;

namespace WebStore.DAL.EF;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<User, IdentityRole<int>, int>(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductStock> ProductStocks => Set<ProductStock>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderProduct> OrderProducts => Set<OrderProduct>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<StationaryStore> StationaryStores => Set<StationaryStore>();
    public DbSet<StationaryStoreEmployee> StationaryStoreEmployees => Set<StationaryStoreEmployee>();

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ValidateEmployees();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ValidateEmployees();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ValidateEmployees()
    {
        foreach (var entry in ChangeTracker.Entries<StationaryStoreEmployee>()
            .Where(x => x.State is EntityState.Added or EntityState.Modified))
            Validator.ValidateObject(entry.Entity, new ValidationContext(entry.Entity), validateAllProperties: true);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<User>().HasDiscriminator<string>("UserType")
            .HasValue<User>("User").HasValue<Customer>("Customer")
            .HasValue<Supplier>("Supplier").HasValue<StationaryStoreEmployee>("Employee");
        b.Entity<User>().Property(x => x.FirstName).IsRequired();
        b.Entity<User>().Property(x => x.LastName).IsRequired();
        b.Entity<StationaryStoreEmployee>().Property(x => x.Position).IsRequired();
        b.Entity<StationaryStoreEmployee>().HasOne(x => x.StationaryStore).WithMany(x => x.Employees)
            .HasForeignKey(x => x.StationaryStoreId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Customer>().HasOne(x => x.BillingAddress).WithMany()
            .HasForeignKey(x => x.BillingAddressId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Customer>().HasOne(x => x.ShippingAddress).WithMany()
            .HasForeignKey(x => x.ShippingAddressId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Address>().HasOne(x => x.Customer).WithMany(x => x.Addresses)
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Address>().HasOne(x => x.StationaryStore).WithMany(x => x.Addresses)
            .HasForeignKey(x => x.StationaryStoreId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Address>().ToTable("Addresses", t => t.HasCheckConstraint("CK_Address_Owner",
            "([CustomerId] IS NOT NULL AND [StationaryStoreId] IS NULL AND [AddressType] IN (0,1,2)) OR ([CustomerId] IS NULL AND [StationaryStoreId] IS NOT NULL AND [AddressType] = 3)"));
        b.Entity<Product>().Property(x => x.Price).HasPrecision(18, 2);
        b.Entity<Product>().HasOne(x => x.Supplier).WithMany(x => x.Products)
            .HasForeignKey(x => x.SupplierId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Product>().HasOne(x => x.Category).WithMany(x => x.Products).HasForeignKey(x => x.CategoryId);
        b.Entity<Product>().ToTable("Products", t => t.HasCheckConstraint("CK_Product_Values", "[Price] >= 0 AND [Weight] >= 0"));
        b.Entity<ProductStock>().HasOne(x => x.Product).WithMany(x => x.ProductStocks).HasForeignKey(x => x.ProductId);
        b.Entity<ProductStock>().ToTable("ProductStocks", t => t.HasCheckConstraint("CK_Stock_Quantity", "[Quantity] >= 0"));
        b.Entity<Order>().HasOne(x => x.Customer).WithMany(x => x.Orders)
            .HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Order>().HasOne(x => x.Invoice).WithMany(x => x.Orders)
            .HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Order>().Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Entity<Order>().ToTable("Orders", t => t.HasCheckConstraint("CK_Order_Dates", "[DeliveryDate] >= [OrderDate] AND [TrackingNumber] >= 0"));
        b.Entity<OrderProduct>().HasKey(x => new { x.OrderId, x.ProductId });
        b.Entity<OrderProduct>().HasOne(x => x.Order).WithMany(x => x.OrderProducts).HasForeignKey(x => x.OrderId);
        b.Entity<OrderProduct>().HasOne(x => x.Product).WithMany(x => x.OrderProducts)
            .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<OrderProduct>().Property(x => x.UnitPrice).HasPrecision(18, 2);
        b.Entity<OrderProduct>().ToTable("OrderProducts", t => t.HasCheckConstraint("CK_OrderProduct_Values", "[Quantity] > 0 AND [UnitPrice] >= 0"));
        b.Entity<Invoice>().HasOne(x => x.Customer).WithMany().HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.NoAction);
        b.Entity<Invoice>().Property(x => x.TotalAmount).HasPrecision(18, 2);
        b.Entity<Invoice>().Property(x => x.Number).HasMaxLength(450).UseCollation("Latin1_General_100_CI_AS");
        b.Entity<Invoice>().HasIndex(x => x.Number).IsUnique();
    }
}
