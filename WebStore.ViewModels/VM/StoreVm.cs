using System.ComponentModel.DataAnnotations;
using WebStore.Model.DataModels;

namespace WebStore.ViewModels.VM;

public class StoreVm
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public List<int> AddressIds { get; set; } = [];
    public List<int> EmployeeIds { get; set; } = [];
}
