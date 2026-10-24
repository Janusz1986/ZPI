using System.Linq.Expressions;
using WebStore.Model.DataModels;
using WebStore.ViewModels.VM;

namespace WebStore.Services.Interfaces;

public interface IOrderService
{
    OrderVm AddOrUpdateOrder(AddOrUpdateOrderVm vm);
    OrderVm? GetOrder(Expression<Func<Order, bool>> filterExpression);
    IEnumerable<OrderVm> GetOrders(Expression<Func<Order, bool>>? filterExpression = null);
}
