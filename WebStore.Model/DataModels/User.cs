namespace WebStore.Model.DataModels;

public class User : Microsoft.AspNetCore.Identity.IdentityUser<int>
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
}
