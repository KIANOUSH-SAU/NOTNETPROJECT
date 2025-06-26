//using Microsoft.AspNetCore.Identity;
//using Microsoft.EntityFrameworkCore;
//using NOTNETPROJECT.Services;
//using NOTNETPROJECT.Data;
//using MongoDB.Driver;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));

////builder.Services.AddDbContext<ApplicationDbContext>(options =>
////    options.UseSqlite(connectionString));

//builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
//    .AddEntityFrameworkStores<ApplicationDbContext>();
//builder.Services.AddControllersWithViews();


//builder.Services.AddTransient<EmailService>();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseMigrationsEndPoint();
//}
//else
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=LandingPage}/{action=Index}/{id?}");
//app.MapRazorPages();

//app.Run();


using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NOTNETPROJECT.Services;
using NOTNETPROJECT.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add try-catch around database configuration
try
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString, sqlOptions =>
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null)));

    builder.Services.AddDatabaseDeveloperPageExceptionFilter();

    builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>();
}
catch (Exception ex)
{
    // Log the error but don't let it crash the application
    Console.WriteLine($"Database configuration error: {ex.Message}");
}

builder.Services.AddControllersWithViews();
builder.Services.AddTransient<EmailService>();

// Configure Kestrel to be more resilient
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000); // Use port 5000 locally
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Safe database initialization
if (app.Environment.IsProduction())
{
    try
    {
        // Apply migrations in a separate scope
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            var dbContext = services.GetRequiredService<ApplicationDbContext>();
            dbContext.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        // Log the error but allow the application to continue running
        Console.WriteLine($"Database migration error: {ex.Message}");
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LandingPage}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();