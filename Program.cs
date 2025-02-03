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
    string connectionString;
    
    // Check for Railway environment variables
    var mysqlHost = Environment.GetEnvironmentVariable("MYSQLHOST");
    if (!string.IsNullOrEmpty(mysqlHost))
    {
        // We're on Railway, construct connection string from environment variables
        var mysqlDatabase = Environment.GetEnvironmentVariable("MYSQLDATABASE");
        var mysqlUser = Environment.GetEnvironmentVariable("MYSQLUSER");
        var mysqlPassword = Environment.GetEnvironmentVariable("MYSQLPASSWORD");
        var mysqlPort = Environment.GetEnvironmentVariable("MYSQLPORT");

        if (string.IsNullOrEmpty(mysqlDatabase) || string.IsNullOrEmpty(mysqlUser) || 
            string.IsNullOrEmpty(mysqlPassword) || string.IsNullOrEmpty(mysqlPort))
        {
            // Log warning but continue without database
            Console.WriteLine("Warning: MySQL environment variables not fully configured. Running without database.");
            return;
        }
        
        connectionString = $"Server={mysqlHost};Port={mysqlPort};Database={mysqlDatabase};User={mysqlUser};Password={mysqlPassword};";
    }
    else
    {
        // Local development or no MySQL configured
        connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            // Log warning but continue without database
            Console.WriteLine("Warning: No database connection string configured. Running without database.");
            return;
        }
    }

    builder.Services.AddDbContext<TennisContext>(options =>
        options.UseMySql(connectionString, 
            ServerVersion.AutoDetect(connectionString),
            mySqlOptions => mySqlOptions.EnableRetryOnFailure())
            .LogTo(Console.WriteLine, LogLevel.Information)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
    );
}
catch (Exception ex)
{
    // Log error but allow application to start without database
    Console.WriteLine($"Database configuration error: {ex.Message}");
    Console.WriteLine("Application will continue without database functionality.");
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

// Initialize the database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<TennisContext>();
        
        // Log the connection string (without password)
        var connectionString = context.Database.GetConnectionString();
        if (connectionString != null)
        {
            var sanitizedConnectionString = connectionString.Replace(
                connectionString.Split(';')
                    .FirstOrDefault(s => s.StartsWith("Password=")) ?? "",
                "Password=*****");
            logger.LogInformation($"Using connection string: {sanitizedConnectionString}");
        }

        logger.LogInformation("Starting database migration...");
        context.Database.Migrate(); 
        logger.LogInformation("Database migrations applied successfully.");

        logger.LogInformation("Starting database initialization...");
        DbInitializer.Initialize(context);
        logger.LogInformation("Database initialized successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
        Console.WriteLine($"Error initializing database: {ex.Message}");
        Console.WriteLine($"Stack trace: {ex.StackTrace}");
        throw;
    }
}

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
