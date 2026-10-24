using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using WebStore.DAL.EF;
using WebStore.Model.DataModels;
using WebStore.Services.ConcreteServices;
using WebStore.Services.Configuration.Profiles;
using WebStore.Services.Interfaces;

namespace WebStore.Services.Configuration;

public static class ServiceRegistration
{
    public static IServiceCollection AddWebStoreServices(this IServiceCollection services, string? autoMapperLicenseKey = null)
    {
        services.AddLogging();
        services.AddAutoMapper(cfg =>
        {
            cfg.LicenseKey = autoMapperLicenseKey ?? Environment.GetEnvironmentVariable("AUTOMAPPER_LICENSE_KEY");
        }, typeof(MainProfile));
        services.AddIdentityCore<User>().AddRoles<IdentityRole<int>>()
            .AddUserValidator<EmployeeFieldsValidator>()
            .AddEntityFrameworkStores<ApplicationDbContext>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IStoreService, StoreService>();
        services.AddScoped<IAddressService, AddressService>();
        return services;
    }
}
