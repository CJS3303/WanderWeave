using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Project1.Areas.Identity.Data;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
var builder = WebApplication.CreateBuilder(args);

var isHeroku = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DYNO"));
if (isHeroku)
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        // Heroku terminates TLS at its router. Trust only the forwarded scheme;
        // dynos are reached through Heroku's routers, whose addresses can change.
        options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });
    builder.Services.AddHttpsRedirection(options => options.HttpsPort = 443);
}

string connectionString = "Server=43.154.234.161;Port=4592;User Id=yuanyuan;Password=yuanlovejc;Database=yuanyuan;";

builder.Services.AddDbContext<Project1IdentityDbContext>(options => options.UseMySql(connectionString, new MySqlServerVersion(new Version(5, 7))));
builder.Services
    .AddAuthentication(o => {
        o.DefaultScheme          = CookieAuthenticationDefaults.AuthenticationScheme;
        o.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddGoogle(opts => {
        opts.ClientId     = builder.Configuration["GoogleKeys:ClientId"];
        opts.ClientSecret = builder.Configuration["GoogleKeys:ClientSecret"];
    });

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<Project1IdentityDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();
builder.Services.AddRazorPages();

var app = builder.Build();

// Restore the public scheme before HTTPS redirects and OAuth callback generation.
if (isHeroku)
{
    app.UseForwardedHeaders();
}

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


app.UseAuthentication(); 
app.UseAuthorization(); 

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
