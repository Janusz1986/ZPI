using System.Linq.Expressions;
using WebStore.Model.DataModels;
using WebStore.ViewModels.VM;

namespace WebStore.Services.Interfaces;

public interface IStoreService
{
    StoreVm AddOrUpdateStore(AddOrUpdateStoreVm vm);
    StoreVm? GetStore(Expression<Func<StationaryStore, bool>> filterExpression);
    IEnumerable<StoreVm> GetStores(Expression<Func<StationaryStore, bool>>? filterExpression = null);
}
