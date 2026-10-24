using System.ComponentModel.DataAnnotations;
using WebStore.Model.DataModels;

namespace WebStore.ViewModels.VM;

public class OrderItemInputVm
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
