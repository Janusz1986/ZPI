using WebStore.Services.Interfaces;

namespace WebStore.Tests;

public class ProductServiceUnitTests
{
    [Fact] public void ReadsSingleFilteredAndAllProductsWithStockSum()
    {
        using var h = new TestHost(); var service = h.Get<IProductService>();
        Assert.Equal(7, service.GetProduct(p => p.Id == h.ProductId)!.Quantity);
        Assert.Equal(0, service.GetProduct(p => p.Id == h.OtherProductId)!.Quantity);
        Assert.Equal(2, service.GetProducts().Count());
        Assert.Single(service.GetProducts(p => p.Price == 500));
        Assert.Null(service.GetProduct(p => p.Id == int.MaxValue));
        Assert.Empty(service.GetProducts(p => p.Id == int.MaxValue));
        Assert.Equal("Monitor Dell 32", service.GetProduct(p => true)!.Name);
    }
    [Theory] [InlineData(null)] [InlineData(0)]
    public void AddsAndPersistsProduct(int? id)
    {
        using var h = new TestHost(); var result = h.Get<IProductService>().AddOrUpdateProduct(h.Product(id));
        Assert.Equal("MacBook Pro", result.Name);
        h.Db.ChangeTracker.Clear();
        Assert.Equal(3, h.Db.Products.Count());
        Assert.Equal(6000m, h.Db.Products.Single(p => p.Name == "MacBook Pro").Price);
    }
    [Fact] public void UpdatesExistingProductAndKeepsStock()
    {
        using var h = new TestHost(); var vm = h.Product(h.ProductId); vm.Price = 2000;
        var result = h.Get<IProductService>().AddOrUpdateProduct(vm);
        Assert.Equal(2000m, result.Price); Assert.Equal(7, result.Quantity);
        h.Db.ChangeTracker.Clear(); Assert.Equal(2, h.Db.Products.Count());
        Assert.Equal(2000m, h.Db.Products.Single(p => p.Id == h.ProductId).Price);
        Assert.Equal(7, h.Db.ProductStocks.Sum(p => p.Quantity));
    }
    [Fact] public void RejectsInvalidIdsNullsAndMissingReferencesWithoutChanges()
    {
        using var h = new TestHost(); var service = h.Get<IProductService>();
        Assert.Throws<ArgumentNullException>(() => service.AddOrUpdateProduct(null!));
        Assert.Throws<ArgumentNullException>(() => service.GetProduct(null!));
        Assert.Throws<ArgumentException>(() => service.AddOrUpdateProduct(h.Product(-1)));
        Assert.Throws<KeyNotFoundException>(() => service.AddOrUpdateProduct(h.Product(int.MaxValue)));
        var vm = h.Product(); vm.SupplierId = h.CustomerId;
        Assert.Throws<ArgumentException>(() => service.AddOrUpdateProduct(vm));
        vm = h.Product(); vm.CategoryId = int.MaxValue;
        Assert.Throws<ArgumentException>(() => service.AddOrUpdateProduct(vm));
        Assert.Equal(2, h.Db.Products.Count());
    }
    [Theory] [InlineData("name")] [InlineData("description")] [InlineData("image")]
    [InlineData("price")] [InlineData("precision")] [InlineData("weight")] [InlineData("nan")] [InlineData("infinity")]
    public void ValidatesProductData(string field)
    {
        using var h = new TestHost(); var vm = h.Product(h.ProductId);
        switch (field)
        {
            case "name": vm.Name = " "; break; case "description": vm.Description = ""; break;
            case "image": vm.ImageBytes = []; break; case "price": vm.Price = -1; break;
            case "precision": vm.Price = 1.001m; break; case "weight": vm.Weight = -1; break;
            case "nan": vm.Weight = float.NaN; break; case "infinity": vm.Weight = float.PositiveInfinity; break;
        }
        Assert.Throws<ArgumentException>(() => h.Get<IProductService>().AddOrUpdateProduct(vm));
        h.Db.ChangeTracker.Clear(); Assert.Equal(1000m, h.Db.Products.Single(p => p.Id == h.ProductId).Price);
    }
}
