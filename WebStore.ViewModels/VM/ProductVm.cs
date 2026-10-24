using System.ComponentModel.DataAnnotations;
using WebStore.Model.DataModels;

namespace WebStore.ViewModels.VM;

public class ProductVm
{
    public string Description { get; set; } = "";
    public byte[] ImageBytes { get; set; } = [];
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public float Weight { get; set; }
    public int Quantity { get; set; }
}
