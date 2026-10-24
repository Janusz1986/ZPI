using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.Interfaces;
using WebStore.ViewModels.VM;

namespace WebStore.Services.ConcreteServices;

public class InvoiceService(ApplicationDbContext db, IMapper mapper, ILogger<InvoiceService> logger)
    : BaseService(db, mapper, logger), IInvoiceService
{
    private IQueryable<Invoice> Query => DbContext.Invoices.Include(x => x.Orders);
    public InvoiceVm AddOrUpdateInvoice(AddOrUpdateInvoiceVm vm) => Save(() =>
    {
        ArgumentNullException.ThrowIfNull(vm);
        ValidateId(vm.Id);
        var invoice = vm.Id > 0 ? Existing(Query.SingleOrDefault(x => x.Id == vm.Id)) : new Invoice();
        var number = Required(vm.Number, nameof(vm.Number)).Trim();
        Ensure(number.Length <= 450, "Numer przekracza techniczną długość indeksu SQL Server (450 znaków).");
        var normalized = number.ToUpperInvariant();
        if (DbContext.Invoices.Any(x => x.Id != invoice.Id && x.Number.ToUpper() == normalized))
            throw new InvalidOperationException("Numer faktury jest zajęty.");
        Ensure(vm.OrderIds is { Count: > 0 }, "Faktura wymaga zamówień.");
        var ids = vm.OrderIds!;
        Ensure(ids.Distinct().Count() == ids.Count, "Zamówienia nie mogą się powtarzać.");
        var orders = DbContext.Orders.Where(x => ids.Contains(x.Id)).ToList();
        Ensure(orders.Count == ids.Count, "Zamówienie nie istnieje.");
        Ensure(orders.Select(x => x.CustomerId).Distinct().Count() == 1, "Zamówienia muszą należeć do jednego klienta.");
        Ensure(vm.Id is null or 0 || invoice.CustomerId == orders[0].CustomerId, "Nie można zmienić klienta faktury.");
        if (orders.Any(x => x.InvoiceId.HasValue && x.InvoiceId != invoice.Id))
            throw new InvalidOperationException("Zamówienie ma już inną fakturę.");
        Ensure(vm.IssueDate != default && orders.All(x => vm.IssueDate >= x.OrderDate), "Nieprawidłowa data faktury.");
        var amount = orders.Sum(x => x.TotalAmount);
        foreach (var removed in invoice.Orders.Where(x => !ids.Contains(x.Id)).ToList())
        {
            invoice.Orders.Remove(removed);
            removed.Invoice = null;
            removed.InvoiceId = null;
        }
        invoice.Number = number;
        invoice.IssueDate = vm.IssueDate;
        invoice.CustomerId = orders[0].CustomerId;
        invoice.TotalAmount = amount;
        foreach (var order in orders)
            if (!invoice.Orders.Any(x => x.Id == order.Id)) invoice.Orders.Add(order);
        if (vm.Id is null or 0) DbContext.Invoices.Add(invoice);
        return invoice;
    }, i => Mapper.Map<InvoiceVm>(i));
    public InvoiceVm? GetInvoice(Expression<Func<Invoice, bool>> filterExpression) => ReadOne<Invoice, InvoiceVm>(Query, filterExpression);
    public IEnumerable<InvoiceVm> GetInvoices(Expression<Func<Invoice, bool>>? filterExpression = null) => ReadMany<Invoice, InvoiceVm>(Query, filterExpression);
}
