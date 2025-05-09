using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project1.Areas.Identity.Data;
var builder = WebApplication.CreateBuilder(args);

string connectionString = "Server=43.154.234.161;Port=4592;User Id=yuanyuan;Password=yuanlovejc;Database=yuanyuan;";

builder.Services.AddDbContext<Project1IdentityDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(5, 7))));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<Project1IdentityDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization(); 

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();