using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Tests;

public class StoreServiceUnitTests
{
    [Theory] [InlineData(null)] [InlineData(0)]
    public void CreatesAndUpdatesStore(int? id)
    {
        using var h = new TestHost(); var service = h.Get<IStoreService>();
        var store = service.AddOrUpdateStore(new AddOrUpdateStoreVm { Id = id, Name = "Sklep", Phone = "123", Email = "sklep@example.test" });
        var updated = service.AddOrUpdateStore(new AddOrUpdateStoreVm { Id = store.Id, Name = "Nowa nazwa" });
        Assert.Equal(store.Id, updated.Id); Assert.Equal("Nowa nazwa", updated.Name);
        Assert.Single(service.GetStores(x => x.Id == store.Id)); Assert.Equal(2, service.GetStores().Count());
        Assert.Null(service.GetStore(x => false)); Assert.Empty(service.GetStores(x => false));
    }
    [Fact] public void KeepsEmployeesAndAddressesWhenUpdating()
    {
        using var h = new TestHost(); var address = h.Address();
        address.CustomerId = null; address.StationaryStoreId = h.StoreId; address.AddressType = AddressType.Store;
        h.Get<IAddressService>().AddOrUpdateAddress(address);
        h.Db.StationaryStoreEmployees.Add(new StationaryStoreEmployee { FirstName = "Ewa", LastName = "Nowak", Position = "Sprzedawca", StationaryStoreId = h.StoreId });
        h.Db.SaveChanges(); h.Db.ChangeTracker.Clear();
        var result = h.Get<IStoreService>().AddOrUpdateStore(new AddOrUpdateStoreVm { Id = h.StoreId, Name = "Nowa nazwa" });
        Assert.Single(result.AddressIds); Assert.Single(result.EmployeeIds);
        Assert.Throws<ArgumentException>(() => h.Get<IStoreService>().AddOrUpdateStore(new AddOrUpdateStoreVm { Name = " " }));
    }
}
