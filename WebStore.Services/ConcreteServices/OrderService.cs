using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Services.ConcreteServices;

public class OrderService(ApplicationDbContext db, IMapper mapper, ILogger<OrderService> logger)
    : BaseService(db, mapper, logger), IOrderService
{
    private IQueryable<Order> Query => DbContext.Orders.Include(x => x.OrderProducts);
    public OrderVm AddOrUpdateOrder(AddOrUpdateOrderVm vm) => Save(() =>
    {
        ArgumentNullException.ThrowIfNull(vm);
        ValidateId(vm.Id);
        var order = vm.Id > 0 ? Existing(Query.SingleOrDefault(x => x.Id == vm.Id)) : new Order();
        if (order.InvoiceId.HasValue) throw new InvalidOperationException("Zafakturowanego zamówienia nie można edytować.");
        Ensure(vm.Id is null or 0 || order.CustomerId == vm.CustomerId, "Nie można zmienić klienta zamówienia.");
        Ensure(DbContext.Customers.Any(x => x.Id == vm.CustomerId), "Klient nie istnieje.");
        Ensure(vm.Items is { Count: > 0 }, "Zamówienie wymaga pozycji.");
        Ensure(vm.OrderDate != default && vm.DeliveryDate >= vm.OrderDate, "Nieprawidłowe daty zamówienia.");
        Ensure(vm.TrackingNumber >= 0, "Numer śledzenia nie może być ujemny.");
        var items = vm.Items!;
        Ensure(items.All(x => x is not null && x.Quantity > 0), "Ilości muszą być dodatnie.");
        Ensure(items.Select(x => x.ProductId).Distinct().Count() == items.Count, "Produkt nie może się powtarzać.");
        var ids = items.Select(x => x.ProductId).ToList();
        var prices = DbContext.Products.Where(x => ids.Contains(x.Id)).ToDictionary(x => x.Id, x => x.Price);
        Ensure(prices.Count == ids.Count, "Produkt nie istnieje.");
        var oldLines = order.OrderProducts.ToDictionary(x => x.ProductId);
        // Cały plan i kwota są sprawdzane przed zmianą śledzonych encji.
        var plan = items.Select(x => new { x.ProductId, x.Quantity,
            UnitPrice = oldLines.TryGetValue(x.ProductId, out var old) ? old.UnitPrice : prices[x.ProductId] }).ToList();
        var amount = plan.Sum(x => checked(x.Quantity * x.UnitPrice));
        order.CustomerId = vm.CustomerId;
        order.OrderDate = vm.OrderDate;
        order.DeliveryDate = vm.DeliveryDate;
        order.TrackingNumber = vm.TrackingNumber;
        order.TotalAmount = amount;
        foreach (var removed in order.OrderProducts.Where(x => !ids.Contains(x.ProductId)).ToList())
        {
            order.OrderProducts.Remove(removed);
            DbContext.OrderProducts.Remove(removed);
        }
        foreach (var item in plan)
        {
            if (oldLines.TryGetValue(item.ProductId, out var line)) line.Quantity = item.Quantity;
            else order.OrderProducts.Add(new OrderProduct { ProductId = item.ProductId, Quantity = item.Quantity, UnitPrice = item.UnitPrice });
        }
        if (vm.Id is null or 0) DbContext.Orders.Add(order);
        return order;
    }, o => Mapper.Map<OrderVm>(o));
    public OrderVm? GetOrder(Expression<Func<Order, bool>> filterExpression) => ReadOne<Order, OrderVm>(Query, filterExpression);
    public IEnumerable<OrderVm> GetOrders(Expression<Func<Order, bool>>? filterExpression = null) => ReadMany<Order, OrderVm>(Query, filterExpression);
}
