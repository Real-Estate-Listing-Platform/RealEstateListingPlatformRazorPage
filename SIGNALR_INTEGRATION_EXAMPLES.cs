// ============================================================
// EXAMPLE: How to Integrate SignalR into Existing Services
// ============================================================

// 1. IN LISTING SERVICE - When Listing is Approved/Rejected
// File: BLL/Services/Implementation/ListingService.cs

public class ListingService : IListingService
{
    // Add SignalR service to constructor
    private readonly ISignalRNotificationService _signalRService; // ADD THIS
    
    public ListingService(
        IListingRepository listingRepository,
        // ... other dependencies ...
        ISignalRNotificationService signalRService) // ADD THIS
    {
        _listingRepository = listingRepository;
        // ... other assignments ...
        _signalRService = signalRService; // ADD THIS
    }
    
    // In your ApproveListingAsync method:
    public async Task<ServiceResult<bool>> ApproveListingAsync(Guid listingId, Guid adminId)
    {
        // ... existing approval logic ...
        
        if (result.Success)
        {
            // ADD THIS: Send real-time notification
            await _signalRService.NotifyListingApprovedAsync(
                listing.ListerId,
                listing.Title
            );
        }
        
        return result;
    }
    
    // In your RejectListingAsync method:
    public async Task<ServiceResult<bool>> RejectListingAsync(Guid listingId, Guid adminId, string reason)
    {
        // ... existing rejection logic ...
        
        if (result.Success)
        {
            // ADD THIS: Send real-time notification
            await _signalRService.NotifyListingRejectedAsync(
                listing.ListerId,
                listing.Title,
                reason
            );
        }
        
        return result;
    }
}

// ============================================================
// 2. IN LEAD SERVICE - When New Lead is Created
// File: BLL/Services/Implementation/LeadService.cs

public class LeadService : ILeadService
{
    // Add SignalR service to constructor
    private readonly ISignalRNotificationService _signalRService; // ADD THIS
    
    public LeadService(
        ILeadRepository leadRepository,
        // ... other dependencies ...
        ISignalRNotificationService signalRService) // ADD THIS
    {
        _leadRepository = leadRepository;
        // ... other assignments ...
        _signalRService = signalRService; // ADD THIS
    }
    
    // In your CreateLeadAsync method:
    public async Task<ServiceResult<Lead>> CreateLeadAsync(Guid listingId, Guid seekerId, string? message)
    {
        // ... existing lead creation logic ...
        
        if (result.Success)
        {
            // ADD THIS: Send real-time notification
            await _signalRService.NotifyNewLeadAsync(
                listing.ListerId,
                listing.Title,
                seeker.DisplayName
            );
        }
        
        return result;
    }
}

// ============================================================
// 3. IN PAYMENT SERVICE - Payment Confirmation
// File: BLL/Services/Implementation/PaymentService.cs

public class PaymentService : IPaymentService
{
    // Add SignalR service to constructor
    private readonly ISignalRNotificationService _signalRService; // ADD THIS
    
    public PaymentService(
        ITransactionRepository transactionRepository,
        // ... other dependencies ...
        ISignalRNotificationService signalRService) // ADD THIS
    {
        _transactionRepository = transactionRepository;
        // ... other assignments ...
        _signalRService = signalRService; // ADD THIS
    }
    
    // In your CompleteTransactionAsync method:
    public async Task<ServiceResult<TransactionDto>> CompleteTransactionAsync(CompleteTransactionDto dto)
    {
        // ... existing completion logic ...
        
        if (result.Success)
        {
            // ADD THIS: Send real-time notification
            await _signalRService.SendToUserAsync(
                transaction.UserId,
                "Payment Successful! 💳",
                $"Your payment of {transaction.Amount:C} VND was processed successfully",
                "success",
                "/Payment/MyTransactions"
            );
        }
        
        return result;
    }
}

// ============================================================
// 4. IN PACKAGE EXPIRATION SERVICE - Package Expiring Soon
// File: BLL/Services/PackageExpirationService.cs

public class PackageExpirationService : BackgroundService
{
    // Add SignalR service
    private readonly ISignalRNotificationService _signalRService; // ADD THIS
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var signalRService = scope.ServiceProvider
                    .GetRequiredService<ISignalRNotificationService>(); // ADD THIS
                
                // ... check expiring packages logic ...
                
                foreach (var package in expiringPackages)
                {
                    var daysRemaining = (package.ExpiryDate - DateTime.UtcNow).Days;
                    
                    // ADD THIS: Send real-time notification
                    await signalRService.NotifyPackageExpiringAsync(
                        package.UserId,
                        package.PackageName,
                        daysRemaining
                    );
                }
            }
            
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}

// ============================================================
// 5. REGISTER SERVICE IN PROGRAM.CS (Already Done!)
// File: RealEstateListingPlatform/Program.cs

// Already added:
builder.Services.AddSingleton<ISignalRNotificationService, SignalRNotificationService>();
builder.Services.AddSignalR();
app.MapHub<NotificationHub>("/notificationHub");

// ============================================================
// 6. CUSTOM NOTIFICATIONS FROM PAGE MODELS
// File: Pages/Admin/Listings.cshtml.cs (Example)

public class ListingsModel : PageModel
{
    private readonly IListingService _listingService;
    private readonly ISignalRNotificationService _signalRService;
    
    public ListingsModel(
        IListingService listingService,
        ISignalRNotificationService signalRService)
    {
        _listingService = listingService;
        _signalRService = signalRService;
    }
    
    public async Task<IActionResult> OnPostBulkApproveAsync(List<Guid> listingIds)
    {
        foreach (var id in listingIds)
        {
            await _listingService.ApproveListingAsync(id, GetAdminId());
        }
        
        // Send notification to all listers
        await _signalRService.SendToRoleAsync(
            "Lister",
            "Bulk Approval Complete",
            $"{listingIds.Count} listings have been approved",
            "success"
        );
        
        return RedirectToPage();
    }
}

// ============================================================
// 7. SYSTEM-WIDE ANNOUNCEMENTS
// File: Pages/Admin/Announcements.cshtml.cs (Example)

public async Task<IActionResult> OnPostAnnouncementAsync(string message)
{
    // Send to all users
    await _signalRService.SendToAllAsync(
        "System Announcement 📢",
        message,
        "info"
    );
    
    // Or send to specific role only
    await _signalRService.SendToRoleAsync(
        "Admin",
        "Admin Announcement",
        message,
        "warning"
    );
    
    return RedirectToPage();
}

// ============================================================
// SUMMARY OF INTEGRATION STEPS:
// 
// 1. Inject ISignalRNotificationService into your service/controller
// 2. Call appropriate notification method when event occurs
// 3. Choose notification type: success, error, warning, info
// 4. Optionally provide URL for "View Details" link
// 5. Test with multiple browser windows/tabs
//
// READY TO USE! 🎉
// ============================================================
