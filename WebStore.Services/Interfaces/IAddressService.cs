using System.Linq.Expressions;
using WebStore.Model.DataModels;
using WebStore.ViewModels.VM;

namespace WebStore.Services.Interfaces;

public interface IAddressService
{
    AddressVm AddOrUpdateAddress(AddOrUpdateAddressVm vm);
    AddressVm? GetAddress(Expression<Func<Address, bool>> filterExpression);
    IEnumerable<AddressVm> GetAddresses(Expression<Func<Address, bool>>? filterExpression = null);
}
