using Microsoft.EntityFrameworkCore;
using TennisClubRanking.Data;
using TennisClubRanking.Services;
using TennisClubRanking.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Configure detailed logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole(options => 
{
    options.LogToStandardErrorThreshold = LogLevel.Debug;
});
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.AspNetCore", LogLevel.Debug);
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Information);
    builder.Logging.AddFilter("TennisClubRanking", LogLevel.Debug);
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
    });

// Configure session with enhanced settings
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Add DistributedMemoryCache for session storage
builder.Services.AddDistributedMemoryCache();

// Add HttpContextAccessor as singleton to ensure consistent access
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Add DbContext with error handling
try
{
    var connectionString = string.Empty;
    var isProduction = builder.Environment.IsProduction();
    var hasDatabase = false;
    
    if (isProduction)
    {
        // Use Railway's environment variables
        var mysqlHost = Environment.GetEnvironmentVariable("MYSQLHOST");
        var mysqlDatabase = Environment.GetEnvironmentVariable("MYSQLDATABASE");
        var mysqlUser = Environment.GetEnvironmentVariable("MYSQLUSER");
        var mysqlPassword = Environment.GetEnvironmentVariable("MYSQLPASSWORD");
        var mysqlPort = Environment.GetEnvironmentVariable("MYSQLPORT");
        
        if (!string.IsNullOrEmpty(mysqlHost) && !string.IsNullOrEmpty(mysqlDatabase) && 
            !string.IsNullOrEmpty(mysqlUser) && !string.IsNullOrEmpty(mysqlPassword) && 
            !string.IsNullOrEmpty(mysqlPort))
        {
            connectionString = $"Server={mysqlHost};Port={mysqlPort};Database={mysqlDatabase};User={mysqlUser};Password={mysqlPassword};AllowUserVariables=true;";
            hasDatabase = true;
        }
        else
        {
            Console.WriteLine("No MySQL configuration found. Running without database.");
        }
    }
    else
    {
        // Use local development connection string
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        hasDatabase = !string.IsNullOrEmpty(connectionString);
    }

    if (hasDatabase)
    {
        builder.Services.AddDbContext<TennisContext>(options =>
        {
            options.UseMySql(connectionString, 
                ServerVersion.AutoDetect(connectionString),
                mySqlOptions => mySqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 10,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null))
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
        });

        // Initialize database only if we have a connection
        var app = builder.Build();
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<TennisContext>();
                context.Database.Migrate();
                DbInitializer.Initialize(context);
                Console.WriteLine("Database initialized successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Database initialization error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
    else
    {
        Console.WriteLine("Running without database functionality.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Configuration error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    Console.WriteLine("Continuing without database functionality.");
}

// Add services
builder.Services.AddScoped<RankingService>();
builder.Services.AddScoped<StatisticsService>();
builder.Services.AddScoped<PromotionRelegationService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddHostedService<PromotionRelegationBackgroundService>();

// Configure the port for Railway deployment
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "auth",
    pattern: "Auth/{action=Login}/{id?}",
    defaults: new { controller = "Auth" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("Application starting on ports 5100 (HTTP) and 7100 (HTTPS)...");
app.Run();
