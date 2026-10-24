using System.ComponentModel.DataAnnotations;
using WebStore.Model.DataModels;

namespace WebStore.ViewModels.VM;

public class InvoiceVm
{
    public int Id { get; set; }
    public string Number { get; set; } = "";
    public DateTime IssueDate { get; set; }
    public int CustomerId { get; set; }
    public List<int> OrderIds { get; set; } = [];
    public decimal TotalAmount { get; set; }
}
