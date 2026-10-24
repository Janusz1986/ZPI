namespace WebStore.Model.DataModels;

public class Product
{
    public int Id { get; set; }
    public string Description { get; set; } = "";
    public byte[] ImageBytes { get; set; } = [];
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public float Weight { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public int SupplierId { get; set; }
    public Supplier Supplier { get; set; } = null!;
    public ICollection<ProductStock> ProductStocks { get; set; } = new List<ProductStock>();
    public ICollection<OrderProduct> OrderProducts { get; set; } = new List<OrderProduct>();
}
