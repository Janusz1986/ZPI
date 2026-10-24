using Microsoft.EntityFrameworkCore;
using WebStore.DAL.EF;
using WebStore.Services.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("WebStore")
        ?? throw new InvalidOperationException("Brak połączenia WebStore.")));
builder.Services.AddWebStoreServices(builder.Configuration["AutoMapper:LicenseKey"]);
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapGet("/api/status", () => Results.Ok(new { application = "WebStore", status = "Połączenie z serwerem działa." }));
app.MapFallbackToFile("index.html");
app.Run();

public partial class Program;
