namespace WebStore.Model.DataModels;

public class Supplier : User
{
    public ICollection<Product> Products { get; set; } = new List<Product>();
}
