using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Services.ConcreteServices;

public class ProductService(ApplicationDbContext db, IMapper mapper, ILogger<ProductService> logger)
    : BaseService(db, mapper, logger), IProductService
{
    private IQueryable<Product> Query => DbContext.Products.Include(x => x.ProductStocks);
    public ProductVm AddOrUpdateProduct(AddOrUpdateProductVm vm) => Save(() =>
    {
        ArgumentNullException.ThrowIfNull(vm);
        ValidateId(vm.Id);
        Required(vm.Name, nameof(vm.Name));
        Required(vm.Description, nameof(vm.Description));
        Ensure(vm.ImageBytes is { Length: > 0 }, "Obraz produktu jest wymagany.");
        Ensure(vm.Price >= 0 && decimal.Round(vm.Price, 2) == vm.Price, "Cena musi być nieujemna i mieć najwyżej dwa miejsca po przecinku.");
        Ensure(float.IsFinite(vm.Weight) && vm.Weight >= 0, "Waga musi być skończona i nieujemna.");
        var product = vm.Id > 0 ? Existing(Query.SingleOrDefault(x => x.Id == vm.Id)) : new Product();
        Ensure(DbContext.Categories.Any(x => x.Id == vm.CategoryId), "Kategoria nie istnieje.");
        Ensure(DbContext.Suppliers.Any(x => x.Id == vm.SupplierId), "Dostawca nie istnieje.");
        Mapper.Map(vm, product);
        if (vm.Id is null or 0) DbContext.Products.Add(product);
        return product;
    }, p => Mapper.Map<ProductVm>(p));
    public ProductVm? GetProduct(Expression<Func<Product, bool>> filterExpression) => ReadOne<Product, ProductVm>(Query, filterExpression);
    public IEnumerable<ProductVm> GetProducts(Expression<Func<Product, bool>>? filterExpression = null) => ReadMany<Product, ProductVm>(Query, filterExpression);
}
