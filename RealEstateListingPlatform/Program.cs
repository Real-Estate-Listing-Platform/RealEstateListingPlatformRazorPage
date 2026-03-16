using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DAL.Models;
using DAL.Repositories;
using DAL.Repositories.Implementation;
using BLL.Services;
using BLL.Services.Implementation;
using BLL.Hubs;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<RealEstateListingPlatformContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("RealEstateListingPlatformContext")));

// Add services to the container.
// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IListingRepository, ListingRepository>();
builder.Services.AddScoped<IPackageRepository, PackageRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IListingViewRepository, ListingViewRepository>();
builder.Services.AddScoped<ILeadRepository, LeadRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IListingSnapshotRepository, ListingSnapshotRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IChatSessionRepository, ChatSessionRepository>();
// Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IListingService, ListingService>();
builder.Services.AddScoped<IPriceHistoryService, PriceHistoryService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IPackageService, PackageService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IPayOSService, PayOSService>();
builder.Services.AddScoped<ILeadService, LeadService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IAdminDashboardService, AdminDashboardService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IValuationService, ValuationService>();
builder.Services.AddScoped<IMarketAnalyticsService, MarketAnalyticsService>();
builder.Services.AddScoped<IValuationReportRepository, ValuationReportRepository>();
builder.Services.AddScoped<IValuationReportService, ValuationReportService>();




// Chatbot Service
builder.Services.AddScoped<IChatbotService, ChatbotService>();

builder.Services.AddHttpClient();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddMemoryCache(); // For OTP caching
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddHostedService<UnverifiedUserCleanupService>();
builder.Services.AddHostedService<PackageExpirationService>(); // Package expiration background job
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
.AddCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Login";
    options.SlidingExpiration = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddAuthorization();

// File upload configuration
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 104857600; // 100MB for multiple files
});

builder.Services.AddSignalR();
builder.Services.AddControllers();
builder.Services.AddRazorPages();
builder.Services.AddControllers();

var app = builder.Build();

// Seed default packages
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<RealEstateListingPlatformContext>();
        
        // Seed packages if none exist
        if (!context.ListingPackages.Any())
        {
            var packages = new List<DAL.Models.ListingPackage>
            {
                // Additional Listing
                new DAL.Models.ListingPackage
                {
                    Id = Guid.NewGuid(),
                    Name = "Additional Listing",
                    Description = "Add one more listing to your account",
                    PackageType = "ADDITIONAL_LISTING",
                    Price = 100000,
                    DurationDays = 30,
                    ListingCount = 1,
                    PhotoLimit = 5,
                    AllowVideo = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Photo Pack 10
                new DAL.Models.ListingPackage
                {
                    Id = Guid.NewGuid(),
                    Name = "Photo Pack +10",
                    Description = "Add 10 extra photos to your listings",
                    PackageType = "PHOTO_PACK",
                    Price = 50000,
                    DurationDays = 30,
                    PhotoLimit = 10,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Photo Pack 20
                new DAL.Models.ListingPackage
                {
                    Id = Guid.NewGuid(),
                    Name = "Photo Pack +20",
                    Description = "Add 20 extra photos to your listings",
                    PackageType = "PHOTO_PACK",
                    Price = 90000,
                    DurationDays = 30,
                    PhotoLimit = 20,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Video Upload
                new DAL.Models.ListingPackage
                {
                    Id = Guid.NewGuid(),
                    Name = "Video Upload",
                    Description = "Enable video uploads for your listing",
                    PackageType = "VIDEO_UPLOAD",
                    Price = 150000,
                    DurationDays = 30,
                    AllowVideo = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Boost 7 Days
                new DAL.Models.ListingPackage
                {
                    Id = Guid.NewGuid(),
                    Name = "Boost 7 Days",
                    Description = "Feature your listing at the top for 7 days",
                    PackageType = "BOOST_LISTING",
                    Price = 200000,
                    BoostDays = 7,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                // Boost 30 Days
                new DAL.Models.ListingPackage
                {
                    Id = Guid.NewGuid(),
                    Name = "Boost 30 Days",
                    Description = "Feature your listing at the top for 30 days",
                    PackageType = "BOOST_LISTING",
                    Price = 500000,
                    BoostDays = 30,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            };
            
            context.ListingPackages.AddRange(packages);
            context.SaveChanges();
            
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Default packages seeded successfully");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding packages");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// Map Razor Pages
app.MapRazorPages();

// Map API Controllers
app.MapControllers();

// Map SignalR Hubs
app.MapHub<DashboardHub>("/hubs/dashboard");

// Map Controllers API
app.MapControllers();

app.Run();
