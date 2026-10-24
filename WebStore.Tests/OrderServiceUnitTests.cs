using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Tests;

public class OrderServiceUnitTests
{
    [Fact] public void CalculatesTotalsPreservesPricesAndReplacesLinesWithoutChangingStock()
    {
        using var h = new TestHost(); var service = h.Get<IOrderService>();
        var order = service.AddOrUpdateOrder(h.Order(0));
        Assert.Equal(2000m, order.TotalAmount);
        h.Get<IProductService>().AddOrUpdateProduct(h.Product(h.ProductId));
        var vm = h.Order(order.Id); vm.Items[0].Quantity = 3;
        vm.Items.Add(new OrderItemInputVm { ProductId = h.OtherProductId, Quantity = 2 });
        order = service.AddOrUpdateOrder(vm);
        Assert.Equal(4000m, order.TotalAmount);
        Assert.Equal(1000m, order.Items.Single(p => p.ProductId == h.ProductId).UnitPrice);
        vm.Items.RemoveAt(0); order = service.AddOrUpdateOrder(vm);
        Assert.Equal(1000m, order.TotalAmount); Assert.Single(order.Items);
        h.Db.ChangeTracker.Clear(); Assert.Single(h.Db.OrderProducts);
        Assert.Equal(7, h.Db.ProductStocks.Sum(p => p.Quantity));
        Assert.Single(service.GetOrders(x => x.Id == order.Id));
        Assert.NotNull(service.GetOrder(x => x.Id == order.Id));
        Assert.Null(service.GetOrder(x => false)); Assert.Empty(service.GetOrders(x => false));
    }
    [Theory] [InlineData("quantity")] [InlineData("duplicate")] [InlineData("product")]
    [InlineData("customer")] [InlineData("date")] [InlineData("tracking")] [InlineData("empty")]
    public void RejectsInvalidOrderWithoutPartialWrite(string field)
    {
        using var h = new TestHost(); var service = h.Get<IOrderService>();
        var original = service.AddOrUpdateOrder(h.Order()); var vm = h.Order(original.Id);
        switch (field)
        {
            case "quantity": vm.Items[0].Quantity = 0; break;
            case "duplicate": vm.Items.Add(new() { ProductId = h.ProductId, Quantity = 1 }); break;
            case "product": vm.Items[0].ProductId = int.MaxValue; break;
            case "customer": vm.CustomerId = h.OtherCustomerId; break;
            case "date": vm.DeliveryDate = vm.OrderDate.AddDays(-1); break;
            case "tracking": vm.TrackingNumber = -1; break;
            case "empty": vm.Items = []; break;
        }
        Assert.Throws<ArgumentException>(() => service.AddOrUpdateOrder(vm));
        h.Db.ChangeTracker.Clear(); Assert.Equal(2000m, h.Db.Orders.Single().TotalAmount);
        Assert.Equal(2, h.Db.OrderProducts.Single().Quantity);
    }
    [Fact] public void RejectsEditingInvoicedOrder()
    {
        using var h = new TestHost(); var service = h.Get<IOrderService>();
        var order = service.AddOrUpdateOrder(h.Order());
        h.Get<IInvoiceService>().AddOrUpdateInvoice(h.Invoice(order.Id));
        Assert.Throws<InvalidOperationException>(() => service.AddOrUpdateOrder(h.Order(order.Id)));
        Assert.Equal(2000m, service.GetOrder(x => x.Id == order.Id)!.TotalAmount);
    }
}
