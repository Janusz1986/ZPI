using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using WebStore.Model.DataModels;

namespace WebStore.Services.Configuration;

public sealed class EmployeeFieldsValidator : IUserValidator<User>
{
    public Task<IdentityResult> ValidateAsync(UserManager<User> manager, User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        if (user is not StationaryStoreEmployee employee)
            return Task.FromResult(IdentityResult.Success);

        var errors = employee.Validate(new ValidationContext(employee))
            .Select(result => new IdentityError
            {
                Code = $"Required{result.MemberNames.Single()}",
                Description = result.ErrorMessage!
            }).ToArray();
        return Task.FromResult(errors.Length == 0 ? IdentityResult.Success : IdentityResult.Failed(errors));
    }
}
