using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using TennisClubRanking.Data;
using TennisClubRanking.Services;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure detailed logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Add cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
    });

// Add session
builder.Services.AddSession();
builder.Services.AddDistributedMemoryCache();

// Add services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<RankingService>();
builder.Services.AddScoped<StatisticsService>();
builder.Services.AddScoped<PromotionRelegationService>();
builder.Services.AddHostedService<PromotionRelegationBackgroundService>();

// Configure database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Build connection string from environment variables for Railway
    var host = builder.Configuration["MYSQLHOST"] ?? "localhost";
    var port = builder.Configuration["MYSQLPORT"] ?? "3306";
    var database = builder.Configuration["MYSQLDATABASE"] ?? "tennis_club";
    var username = builder.Configuration["MYSQLUSER"] ?? "root";
    var password = builder.Configuration["MYSQLPASSWORD"] ?? "password";

    connectionString = $"Server={host};Port={port};Database={database};User={username};Password={password};";
}

builder.Services.AddDbContext<TennisContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString),
        mySqlOptions => mySqlOptions.EnableRetryOnFailure());
});

// Configure port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"Configuring to listen on port: {port}");
builder.WebHost.UseUrls($"http://+:{port}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Ensure database is created and migrations are applied
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TennisContext>();
    context.Database.Migrate();
}

Console.WriteLine("=== Application Starting ===");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Server URLs: {string.Join(", ", app.Urls)}");

app.Run();
