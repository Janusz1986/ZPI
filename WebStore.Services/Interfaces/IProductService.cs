using System.Linq.Expressions;
using WebStore.Model.DataModels;
using WebStore.ViewModels.VM;

namespace WebStore.Services.Interfaces;

public interface IProductService
{
    ProductVm AddOrUpdateProduct(AddOrUpdateProductVm vm);
    ProductVm? GetProduct(Expression<Func<Product, bool>> filterExpression);
    IEnumerable<ProductVm> GetProducts(Expression<Func<Product, bool>>? filterExpression = null);
}
