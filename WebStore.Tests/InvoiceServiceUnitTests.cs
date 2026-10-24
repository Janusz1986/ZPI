using WebStore.Services.Interfaces;

namespace WebStore.Tests;

public class InvoiceServiceUnitTests
{
    [Fact] public void GroupsOrdersAndReplacesLinksAtomically()
    {
        using var h = new TestHost(); var orders = h.Get<IOrderService>(); var service = h.Get<IInvoiceService>();
        var first = orders.AddOrUpdateOrder(h.Order()); var second = orders.AddOrUpdateOrder(h.Order());
        var vm = h.Invoice(first.Id, second.Id); var invoice = service.AddOrUpdateInvoice(vm);
        Assert.Equal(4000m, invoice.TotalAmount); Assert.Equal(2, invoice.OrderIds.Count);
        Assert.Equal(h.CustomerId, invoice.CustomerId);
        vm.Id = invoice.Id; vm.Number = "FV/2026/2"; vm.OrderIds = [second.Id];
        invoice = service.AddOrUpdateInvoice(vm);
        Assert.Equal(2000m, invoice.TotalAmount); Assert.Single(invoice.OrderIds);
        h.Db.ChangeTracker.Clear(); Assert.Null(h.Db.Orders.Single(x => x.Id == first.Id).InvoiceId);
        Assert.Equal(invoice.Id, h.Db.Orders.Single(x => x.Id == second.Id).InvoiceId);
        Assert.Single(service.GetInvoices(x => x.Number == "FV/2026/2"));
        Assert.NotNull(service.GetInvoice(x => x.Id == invoice.Id));
        Assert.Null(service.GetInvoice(x => false)); Assert.Empty(service.GetInvoices(x => false));
    }
    [Theory] [InlineData("number")] [InlineData("date")] [InlineData("empty")] [InlineData("missing")]
    [InlineData("duplicate")] [InlineData("customer")]
    public void RejectsInvalidInvoiceWithoutChangingOrders(string field)
    {
        using var h = new TestHost(); var order = h.Get<IOrderService>().AddOrUpdateOrder(h.Order());
        var vm = h.Invoice(order.Id);
        switch (field)
        {
            case "number": vm.Number = " "; break; case "date": vm.IssueDate = new DateTime(2020, 1, 1); break;
            case "empty": vm.OrderIds = []; break; case "missing": vm.OrderIds = [int.MaxValue]; break;
            case "duplicate": vm.OrderIds.Add(order.Id); break;
            case "customer":
                var other = h.Order(); other.CustomerId = h.OtherCustomerId;
                vm.OrderIds.Add(h.Get<IOrderService>().AddOrUpdateOrder(other).Id); break;
        }
        Assert.Throws<ArgumentException>(() => h.Get<IInvoiceService>().AddOrUpdateInvoice(vm));
        h.Db.ChangeTracker.Clear(); Assert.Empty(h.Db.Invoices);
        Assert.All(h.Db.Orders, x => Assert.Null(x.InvoiceId));
    }
    [Fact] public void RejectsDuplicateNumbersAndOrdersAlreadyInvoiced()
    {
        using var h = new TestHost(); var service = h.Get<IInvoiceService>();
        var first = h.Get<IOrderService>().AddOrUpdateOrder(h.Order());
        service.AddOrUpdateInvoice(h.Invoice(first.Id));
        var second = h.Get<IOrderService>().AddOrUpdateOrder(h.Order());
        var vm = h.Invoice(second.Id); vm.Number = "  fv/2026/1  ";
        Assert.Throws<InvalidOperationException>(() => service.AddOrUpdateInvoice(vm));
        vm = h.Invoice(first.Id); vm.Number = "FV/2026/2";
        Assert.Throws<InvalidOperationException>(() => service.AddOrUpdateInvoice(vm));
        Assert.Single(h.Db.Invoices);
    }
}
