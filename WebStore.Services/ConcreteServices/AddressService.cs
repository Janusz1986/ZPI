using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Services.ConcreteServices;

public class AddressService(ApplicationDbContext db, IMapper mapper, ILogger<AddressService> logger)
    : BaseService(db, mapper, logger), IAddressService
{
    private IQueryable<Address> Query => DbContext.Addresses.Include(x => x.Customer);
    public AddressVm AddOrUpdateAddress(AddOrUpdateAddressVm vm) => Save(() =>
    {
        ArgumentNullException.ThrowIfNull(vm);
        ValidateId(vm.Id);
        var address = vm.Id > 0 ? Existing(Query.SingleOrDefault(x => x.Id == vm.Id)) : new Address();
        Required(vm.Country, nameof(vm.Country)); Required(vm.PostalCode, nameof(vm.PostalCode));
        Required(vm.City, nameof(vm.City)); Required(vm.Street, nameof(vm.Street));
        Required(vm.BuildingNumber, nameof(vm.BuildingNumber));
        Ensure(Enum.IsDefined(vm.AddressType), "Nieprawidłowy typ adresu.");
        Ensure(vm.CustomerId.HasValue != vm.StationaryStoreId.HasValue, "Adres wymaga dokładnie jednego właściciela.");
        Ensure(vm.Id is null or 0 || (address.CustomerId == vm.CustomerId && address.StationaryStoreId == vm.StationaryStoreId), "Nie można zmienić właściciela adresu.");
        Customer? customer = null;
        if (vm.CustomerId.HasValue)
        {
            customer = DbContext.Customers.SingleOrDefault(x => x.Id == vm.CustomerId);
            Ensure(customer is not null, "Klient nie istnieje.");
            Ensure(vm.AddressType != AddressType.Store, "Adres klienta nie może mieć typu Store.");
        }
        else
        {
            Ensure(DbContext.StationaryStores.Any(x => x.Id == vm.StationaryStoreId), "Sklep nie istnieje.");
            Ensure(vm.AddressType == AddressType.Store, "Adres sklepu musi mieć typ Store.");
            Ensure(!vm.MakeDefaultBilling && !vm.MakeDefaultShipping, "Adres sklepu nie może być domyślnym adresem klienta.");
        }
        var billing = vm.AddressType is AddressType.Billing or AddressType.BillingAndShipping;
        var shipping = vm.AddressType is AddressType.Shipping or AddressType.BillingAndShipping;
        Ensure(!vm.MakeDefaultBilling || billing, "Adres nie ma typu rozliczeniowego.");
        Ensure(!vm.MakeDefaultShipping || shipping, "Adres nie ma typu dostawy.");
        Ensure(customer?.BillingAddressId != address.Id || billing, "Zmień najpierw wybrany adres rozliczeniowy.");
        Ensure(customer?.ShippingAddressId != address.Id || shipping, "Zmień najpierw wybrany adres dostawy.");
        Mapper.Map(vm, address);
        address.Customer = customer;
        if (vm.Id is null or 0) DbContext.Addresses.Add(address);
        if (customer is not null)
        {
            if (vm.MakeDefaultBilling) customer.BillingAddress = address;
            if (vm.MakeDefaultShipping) customer.ShippingAddress = address;
        }
        return address;
    }, a => Mapper.Map<AddressVm>(a));
    public AddressVm? GetAddress(Expression<Func<Address, bool>> filterExpression) => ReadOne<Address, AddressVm>(Query, filterExpression);
    public IEnumerable<AddressVm> GetAddresses(Expression<Func<Address, bool>>? filterExpression = null) => ReadMany<Address, AddressVm>(Query, filterExpression);
}
