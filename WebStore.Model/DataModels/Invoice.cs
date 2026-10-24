namespace WebStore.Model.DataModels;

public class Invoice
{
    public int Id { get; set; }
    public string Number { get; set; } = "";
    public DateTime IssueDate { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
