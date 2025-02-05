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

// Configure forwarded headers and HTTPS
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                              Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

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
        // Use Railway's environment variables for MySQL
        var mysqlHost = Environment.GetEnvironmentVariable("MYSQLHOST");
        var mysqlDatabase = Environment.GetEnvironmentVariable("MYSQLDATABASE");
        var mysqlUser = Environment.GetEnvironmentVariable("MYSQLUSER");
        var mysqlPassword = Environment.GetEnvironmentVariable("MYSQLPASSWORD");
        var mysqlPort = Environment.GetEnvironmentVariable("MYSQLPORT") ?? "3306"; // Default MySQL port if not specified
        
        Console.WriteLine("Railway MySQL Configuration:");
        Console.WriteLine($"MYSQLHOST: {!string.IsNullOrEmpty(mysqlHost)}");
        Console.WriteLine($"MYSQLDATABASE: {!string.IsNullOrEmpty(mysqlDatabase)}");
        Console.WriteLine($"MYSQLUSER: {!string.IsNullOrEmpty(mysqlUser)}");
        Console.WriteLine($"MYSQLPORT: {mysqlPort}");
        Console.WriteLine($"MYSQLPASSWORD: {!string.IsNullOrEmpty(mysqlPassword)}");
        
        if (!string.IsNullOrEmpty(mysqlHost) && !string.IsNullOrEmpty(mysqlDatabase) && 
            !string.IsNullOrEmpty(mysqlUser) && !string.IsNullOrEmpty(mysqlPassword))
        {
            connectionString = $"Server={mysqlHost};Port={mysqlPort};Database={mysqlDatabase};User={mysqlUser};Password={mysqlPassword};AllowUserVariables=true;";
            hasDatabase = true;
            Console.WriteLine("MySQL connection string configured successfully");
        }
        else
        {
            Console.WriteLine("ERROR: Missing required MySQL environment variables");
            foreach (var ev in new[] { "MYSQLHOST", "MYSQLDATABASE", "MYSQLUSER", "MYSQLPASSWORD" })
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(ev)))
                {
                    Console.WriteLine($"Missing: {ev}");
                }
            }
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

// Configure the application port for Railway deployment
var appPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrEmpty(appPort))
{
    Console.WriteLine($"Using Railway PORT: {appPort}");
    builder.WebHost.UseUrls($"http://+:{appPort}");
}
else
{
    Console.WriteLine("PORT environment variable not found, using default port 8080");
    builder.WebHost.UseUrls("http://+:8080");
}

var app = builder.Build();

// Initialize database if we have one configured
if (app.Services.GetService<TennisContext>() != null)
{
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    
    // Enable HTTPS Redirection and HSTS
    app.UseHsts();
    app.UseHttpsRedirection();
    
    // Use forwarded headers
    app.UseForwardedHeaders();
}

// Trust the Railway proxy
app.Use((context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-Forwarded-Proto", out var proto))
    {
        if (proto == "https")
        {
            context.Request.Scheme = "https";
        }
    }
    
    if (context.Request.Headers.TryGetValue("X-Forwarded-Host", out var host))
    {
        context.Request.Host = new HostString(host);
    }
    
    return next();
});

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

Console.WriteLine("Application starting...");
app.Run();
