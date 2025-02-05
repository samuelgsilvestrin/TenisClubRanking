var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure detailed logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Configure authentication
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

// Configure database
var isProduction = builder.Environment.IsProduction();
if (isProduction)
{
    // Use Railway's environment variables for MySQL
    var mysqlHost = Environment.GetEnvironmentVariable("MYSQLHOST");
    var mysqlDatabase = Environment.GetEnvironmentVariable("MYSQLDATABASE");
    var mysqlUser = Environment.GetEnvironmentVariable("MYSQLUSER");
    var mysqlPassword = Environment.GetEnvironmentVariable("MYSQLPASSWORD");
    var mysqlPort = Environment.GetEnvironmentVariable("MYSQLPORT") ?? "3306";

    Console.WriteLine("=== Railway Configuration ===");
    Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
    Console.WriteLine($"MySQL Host Available: {!string.IsNullOrEmpty(mysqlHost)}");
    Console.WriteLine($"MySQL Database Available: {!string.IsNullOrEmpty(mysqlDatabase)}");
    Console.WriteLine($"MySQL User Available: {!string.IsNullOrEmpty(mysqlUser)}");
    Console.WriteLine($"MySQL Port: {mysqlPort}");

    if (!string.IsNullOrEmpty(mysqlHost) && !string.IsNullOrEmpty(mysqlDatabase) && 
        !string.IsNullOrEmpty(mysqlUser) && !string.IsNullOrEmpty(mysqlPassword))
    {
        var connectionString = $"Server={mysqlHost};Port={mysqlPort};Database={mysqlDatabase};User={mysqlUser};Password={mysqlPassword};AllowUserVariables=true;";
        builder.Services.AddDbContext<TennisContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
        Console.WriteLine("Database context configured successfully");
    }
    else
    {
        Console.WriteLine("ERROR: Missing required MySQL environment variables");
    }
}
else
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        builder.Services.AddDbContext<TennisContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));
    }
}

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

Console.WriteLine("=== Application Starting ===");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Server URLs: {string.Join(", ", app.Urls)}");

app.Run();
