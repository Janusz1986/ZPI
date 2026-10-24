using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;

namespace WebStore.Tests;

public class AddressServiceUnitTests
{
    [Fact] public void CreatesSelectsSwitchesAndUpdatesCustomerAddresses()
    {
        using var h = new TestHost(); var service = h.Get<IAddressService>();
        var vm = h.Address(0); vm.MakeDefaultBilling = true; vm.MakeDefaultShipping = true;
        var first = service.AddOrUpdateAddress(vm);
        Assert.True(first.IsDefaultBilling); Assert.True(first.IsDefaultShipping);
        vm = h.Address(); vm.MakeDefaultBilling = true; vm.Street = "Krótka";
        var second = service.AddOrUpdateAddress(vm);
        Assert.True(second.IsDefaultBilling); Assert.False(second.IsDefaultShipping);
        h.Db.ChangeTracker.Clear();
        Assert.False(service.GetAddress(x => x.Id == first.Id)!.IsDefaultBilling);
        Assert.True(service.GetAddress(x => x.Id == first.Id)!.IsDefaultShipping);
        vm.Id = second.Id; vm.MakeDefaultBilling = false; vm.City = "Kraków";
        var updated = service.AddOrUpdateAddress(vm);
        Assert.True(updated.IsDefaultBilling); Assert.Equal("Kraków", updated.City);
        Assert.Equal(2, service.GetAddresses().Count());
        Assert.Single(service.GetAddresses(x => x.Id == first.Id));
        Assert.Null(service.GetAddress(x => false)); Assert.Empty(service.GetAddresses(x => false));
    }
    [Fact] public void SupportsMultipleStoreAddressesAndBlocksWrongOwnerOrRole()
    {
        using var h = new TestHost(); var service = h.Get<IAddressService>();
        var vm = h.Address(); vm.CustomerId = null; vm.StationaryStoreId = h.StoreId; vm.AddressType = AddressType.Store;
        var first = service.AddOrUpdateAddress(vm); service.AddOrUpdateAddress(vm);
        Assert.Equal(2, h.Get<IStoreService>().GetStore(x => x.Id == h.StoreId)!.AddressIds.Count);
        vm.Id = first.Id; vm.CustomerId = h.CustomerId; vm.StationaryStoreId = null; vm.AddressType = AddressType.Billing;
        Assert.Throws<ArgumentException>(() => service.AddOrUpdateAddress(vm));
        vm = h.Address(); vm.MakeDefaultBilling = true; var billing = service.AddOrUpdateAddress(vm);
        vm.Id = billing.Id; vm.MakeDefaultBilling = false; vm.AddressType = AddressType.Shipping;
        Assert.Throws<ArgumentException>(() => service.AddOrUpdateAddress(vm));
    }
    [Theory] [InlineData("both")] [InlineData("none")] [InlineData("missing")]
    [InlineData("type")] [InlineData("role")] [InlineData("country")] [InlineData("city")]
    public void ValidatesAddressWithoutWriting(string field)
    {
        using var h = new TestHost(); var vm = h.Address();
        switch (field)
        {
            case "both": vm.StationaryStoreId = h.StoreId; break;
            case "none": vm.CustomerId = null; break;
            case "missing": vm.CustomerId = int.MaxValue; break;
            case "type": vm.AddressType = AddressType.Store; break;
            case "role": vm.AddressType = AddressType.Shipping; vm.MakeDefaultBilling = true; break;
            case "country": vm.Country = " "; break; case "city": vm.City = ""; break;
        }
        Assert.Throws<ArgumentException>(() => h.Get<IAddressService>().AddOrUpdateAddress(vm));
        Assert.Empty(h.Db.Addresses);
    }
}
