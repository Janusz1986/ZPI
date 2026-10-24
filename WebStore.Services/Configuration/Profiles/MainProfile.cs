using AutoMapper;
using WebStore.Model.DataModels;
using WebStore.ViewModels.VM;

namespace WebStore.Services.Configuration.Profiles;

public class MainProfile : Profile
{
    public MainProfile()
    {
        CreateMap<Product, ProductVm>().ForMember(x => x.Quantity, o => o.MapFrom(x => x.ProductStocks.Sum(s => s.Quantity)));
        CreateMap<AddOrUpdateProductVm, Product>(MemberList.None)
            .ForMember(x => x.Id, o => o.Ignore());
        CreateMap<OrderProduct, OrderItemVm>();
        CreateMap<Order, OrderVm>().ForMember(x => x.Items, o => o.MapFrom(x => x.OrderProducts.OrderBy(p => p.ProductId)));
        CreateMap<Invoice, InvoiceVm>().ForMember(x => x.OrderIds, o => o.MapFrom(x => x.Orders.OrderBy(p => p.Id).Select(p => p.Id)));
        CreateMap<StationaryStore, StoreVm>()
            .ForMember(x => x.AddressIds, o => o.MapFrom(x => x.Addresses.OrderBy(p => p.Id).Select(p => p.Id)))
            .ForMember(x => x.EmployeeIds, o => o.MapFrom(x => x.Employees.OrderBy(p => p.Id).Select(p => p.Id)));
        CreateMap<AddOrUpdateStoreVm, StationaryStore>(MemberList.None).ForMember(x => x.Id, o => o.Ignore());
        CreateMap<Address, AddressVm>()
            .ForMember(x => x.IsDefaultBilling, o => o.MapFrom(x => x.Customer != null && x.Customer.BillingAddressId == x.Id))
            .ForMember(x => x.IsDefaultShipping, o => o.MapFrom(x => x.Customer != null && x.Customer.ShippingAddressId == x.Id));
        CreateMap<AddOrUpdateAddressVm, Address>(MemberList.None).ForMember(x => x.Id, o => o.Ignore());
    }
}
