using FirebaseAdmin;
using PROG7311_P2.Models;
using PROG7311_P2.Services;
using PROG7311_P2.Middleware;
using Google.Apis.Auth.OAuth2;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/farmcentral-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting Farm Central application");

    // Add services to the container.
    builder.Services.AddControllersWithViews();

    builder.Services.AddDistributedMemoryCache();

    // Add rate limiting
    builder.Services.AddRateLimiter(options =>
    {
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 100,
                    Window = TimeSpan.FromMinutes(1)
                }));
    });

    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromHours(1);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

    // Register Firebase services
    builder.Services.AddScoped<IFirebaseDatabaseService, FirebaseDatabaseService>();
    builder.Services.AddScoped<IFirebaseAuthService, FirebaseAuthService>();

    // Register repositories and services
    builder.Services.AddScoped<IProductRepository, ProductRepository>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<IUserService, UserService>();

    // Initialize Firebase with configuration
    try
    {
        var firebaseConfig = builder.Configuration.GetSection("Firebase");
        var credentialsPath = firebaseConfig["CredentialsPath"];
        
        GoogleCredential credential;
        
        // Check if we're in production (Render) and have environment variable credentials
        var firebaseCredentialsJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON");
        if (!string.IsNullOrEmpty(firebaseCredentialsJson))
        {
            // Use environment variable credentials (production)
            credential = GoogleCredential.FromJson(firebaseCredentialsJson);
            Log.Information("Firebase initialized with environment variable credentials");
        }
        else if (!string.IsNullOrEmpty(credentialsPath) && File.Exists(credentialsPath))
        {
            // Use file-based credentials (development)
            credential = GoogleCredential.FromFile(credentialsPath);
            Log.Information("Firebase initialized with file-based credentials");
        }
        else
        {
            throw new InvalidOperationException("Firebase credentials not found. Set FIREBASE_CREDENTIALS_JSON environment variable or provide valid credentials file path.");
        }
        
        FirebaseApp.Create(new AppOptions()
        {
            Credential = credential
        });
        
        builder.Services.AddSingleton(FirebaseApp.DefaultInstance);
        Log.Information("Firebase initialized successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "Failed to initialize Firebase");
        // In production, you might want to handle this differently
    }

    var app = builder.Build();



    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    else
    {
        app.UseDeveloperExceptionPage();
    }

    // Add security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        await next();
    });

    // Add global exception handler middleware
    app.UseMiddleware<GlobalExceptionHandler>();

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();
    app.UseSession();
    app.UseRateLimiter();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    Log.Information("Farm Central application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
