using AutoMapper;
using Microsoft.AspNetCore.Identity;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Tests;

public class ContractTests
{
    [Fact] public void MappingAndIdentityAreConfigured()
    {
        using var h = new TestHost(); h.Get<IMapper>().ConfigurationProvider.AssertConfigurationIsValid();
        Assert.NotNull(h.Get<UserManager<User>>()); Assert.NotNull(h.Get<RoleManager<IdentityRole<int>>>());
    }
    [Theory] [InlineData("Order")] [InlineData("Invoice")] [InlineData("Store")] [InlineData("Address")]
    public void AdditionalServicesHaveConsistentIdNullAndEmptyReadBehavior(string name)
    {
        using var h = new TestHost();
        switch (name)
        {
            case "Order":
                var o = h.Get<IOrderService>();
                Assert.Throws<ArgumentNullException>(() => o.AddOrUpdateOrder(null!));
                Assert.Throws<ArgumentNullException>(() => o.GetOrder(null!));
                Assert.Throws<ArgumentException>(() => o.AddOrUpdateOrder(h.Order(-1)));
                Assert.Throws<KeyNotFoundException>(() => o.AddOrUpdateOrder(h.Order(int.MaxValue)));
                Assert.NotNull(o.AddOrUpdateOrder(h.Order(0))); break;
            case "Invoice":
                var i = h.Get<IInvoiceService>();
                Assert.Throws<ArgumentNullException>(() => i.AddOrUpdateInvoice(null!));
                Assert.Throws<ArgumentNullException>(() => i.GetInvoice(null!));
                var ivm = h.Invoice(); ivm.Id = -1;
                Assert.Throws<ArgumentException>(() => i.AddOrUpdateInvoice(ivm)); ivm.Id = int.MaxValue;
                Assert.Throws<KeyNotFoundException>(() => i.AddOrUpdateInvoice(ivm));
                var order = h.Get<IOrderService>().AddOrUpdateOrder(h.Order());
                ivm = h.Invoice(order.Id); ivm.Id = 0; Assert.NotNull(i.AddOrUpdateInvoice(ivm)); break;
            case "Store":
                var s = h.Get<IStoreService>();
                Assert.Throws<ArgumentNullException>(() => s.AddOrUpdateStore(null!));
                Assert.Throws<ArgumentNullException>(() => s.GetStore(null!));
                Assert.Throws<ArgumentException>(() => s.AddOrUpdateStore(new AddOrUpdateStoreVm { Id = -1 }));
                Assert.Throws<KeyNotFoundException>(() => s.AddOrUpdateStore(new AddOrUpdateStoreVm { Id = int.MaxValue, Name = "Sklep" })); break;
            case "Address":
                var a = h.Get<IAddressService>();
                Assert.Throws<ArgumentNullException>(() => a.AddOrUpdateAddress(null!));
                Assert.Throws<ArgumentNullException>(() => a.GetAddress(null!));
                Assert.Throws<ArgumentException>(() => a.AddOrUpdateAddress(h.Address(-1)));
                Assert.Throws<KeyNotFoundException>(() => a.AddOrUpdateAddress(h.Address(int.MaxValue))); break;
        }
    }
}
