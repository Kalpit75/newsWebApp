using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using newsWebApp.Data;
using newsWebApp.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(); // Changed from AddRazorPages()

// Single database for both Identity and News data
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false; // Set to false for development
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>();



builder.Services.AddScoped<RssFeedService>();
builder.Services.AddHostedService<BackgroundRssFeedService>();

//google OAuth
builder.Services.AddAuthentication().AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Changed from "/Error"
    app.UseHsts();
}



app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); 
app.UseAuthorization();

// Add MVC routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages(); // Keep this for Identity pages

app.Run();
