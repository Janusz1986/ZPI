using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Services.ConcreteServices;

public class StoreService(ApplicationDbContext db, IMapper mapper, ILogger<StoreService> logger)
    : BaseService(db, mapper, logger), IStoreService
{
    private IQueryable<StationaryStore> Query => DbContext.StationaryStores.Include(x => x.Addresses).Include(x => x.Employees).AsSplitQuery();
    public StoreVm AddOrUpdateStore(AddOrUpdateStoreVm vm) => Save(() =>
    {
        ArgumentNullException.ThrowIfNull(vm);
        ValidateId(vm.Id);
        Required(vm.Name, nameof(vm.Name));
        var store = vm.Id > 0 ? Existing(Query.SingleOrDefault(x => x.Id == vm.Id)) : new StationaryStore();
        Mapper.Map(vm, store);
        if (vm.Id is null or 0) DbContext.StationaryStores.Add(store);
        return store;
    }, s => Mapper.Map<StoreVm>(s));
    public StoreVm? GetStore(Expression<Func<StationaryStore, bool>> filterExpression) => ReadOne<StationaryStore, StoreVm>(Query, filterExpression);
    public IEnumerable<StoreVm> GetStores(Expression<Func<StationaryStore, bool>>? filterExpression = null) => ReadMany<StationaryStore, StoreVm>(Query, filterExpression);
}
