using B2B.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Connection string comes from appsettings.json (never hard-coded).
var cs = builder.Configuration.GetConnectionString("B2BDb")
         ?? throw new InvalidOperationException("Connection string 'B2BDb' is missing in appsettings.json.");
builder.Services.AddDbContext<B2BContext>(o => o.UseSqlServer(cs));
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrderService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/AccessDenied";
        o.ExpireTimeSpan = TimeSpan.FromHours(8);
        o.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Code First: create / upgrade the database and seed data on start-up.
using (var scope = app.Services.CreateScope())
    scope.ServiceProvider.GetRequiredService<B2BContext>().Database.Migrate();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Catalog}/{action=Index}/{id?}");

app.Run();
