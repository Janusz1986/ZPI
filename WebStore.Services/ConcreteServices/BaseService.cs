using System.Data;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;

namespace WebStore.Services.ConcreteServices;

public abstract class BaseService(ApplicationDbContext dbContext, IMapper mapper, ILogger logger)
{
    protected readonly ApplicationDbContext DbContext = dbContext;
    protected readonly IMapper Mapper = mapper;
    protected readonly ILogger Logger = logger;

    protected T Execute<T>(Func<T> action)
    {
        try { return action(); }
        catch (Exception ex) { Logger.LogError(ex, "Błąd operacji usługi {Service}.", GetType().Name); throw; }
    }

    protected TVm Save<TEntity, TVm>(Func<TEntity> prepare, Func<TEntity, TVm> result) => Execute(() =>
    {
        // Usługa nie zapisuje przypadkowo cudzych oczekujących zmian kontekstu.
        if (DbContext.ChangeTracker.HasChanges())
            throw new InvalidOperationException("Przed wywołaniem usługi zapisz lub wycofaj zmiany kontekstu.");
        using var transaction = DbContext.Database.IsRelational()
            ? DbContext.Database.BeginTransaction(IsolationLevel.Serializable) : null;
        try
        {
            // Brak oczekujących zmian potwierdzono wyżej. Odczyty reguł muszą pochodzić
            // z bieżącej transakcji, a nie ze stanu zapamiętanego w poprzedniej operacji.
            DbContext.ChangeTracker.Clear();
            var entity = prepare();
            DbContext.SaveChanges();
            var vm = result(entity);
            transaction?.Commit();
            return vm;
        }
        catch
        {
            transaction?.Rollback();
            DbContext.ChangeTracker.Clear();
            throw;
        }
    });

    protected TVm? ReadOne<TEntity, TVm>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>> filter)
        where TEntity : class where TVm : class => Execute(() =>
    {
        ArgumentNullException.ThrowIfNull(filter);
        var entity = query.AsNoTracking().Where(filter).OrderBy(x => EF.Property<int>(x, "Id")).FirstOrDefault();
        return entity is null ? null : Mapper.Map<TVm>(entity);
    });

    protected IEnumerable<TVm> ReadMany<TEntity, TVm>(IQueryable<TEntity> query, Expression<Func<TEntity, bool>>? filter)
        where TEntity : class => Execute(() =>
    {
        if (filter is not null) query = query.Where(filter);
        return Mapper.Map<List<TVm>>(query.AsNoTracking().OrderBy(x => EF.Property<int>(x, "Id")).ToList());
    });

    protected static void ValidateId(int? id)
    {
        if (id < 0) throw new ArgumentException("Id nie może być ujemne.", nameof(id));
    }
    protected static string Required(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value)) throw new ArgumentException("Pole jest wymagane.", name);
        return value;
    }
    protected static void Ensure(bool condition, string message)
    {
        if (!condition) throw new ArgumentException(message);
    }
    protected static T Existing<T>(T? entity) where T : class => entity
        ?? throw new KeyNotFoundException("Nie znaleziono rekordu do aktualizacji.");
}
