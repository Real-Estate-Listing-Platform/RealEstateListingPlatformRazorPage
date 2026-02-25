# SignalR Real-Time Notifications - Documentation

## 📡 Tổng Quan

SignalR đã được tích hợp vào Real Estate Listing Platform để cung cấp thông báo real-time cho users.

## ✅ Đã Thêm Vào Project

### 1. **Backend Components**

#### `Hubs/NotificationHub.cs`
- SignalR Hub chính để xử lý connections
- Auto-add users vào groups theo UserID và Role
- Method `MarkAsRead()` để đánh dấu thông báo đã đọc

#### `Services/SignalRNotificationService.cs`
- Service để gửi notifications qua SignalR
- **Methods:**
  - `SendToUserAsync()` - Gửi đến 1 user cụ thể
  - `SendToRoleAsync()` - Gửi đến tất cả users có role
  - `SendToAllAsync()` - Broadcast đến tất cả
  - `NotifyNewLeadAsync()` - Thông báo lead mới
  - `NotifyListingApprovedAsync()` - Thông báo listing approved
  - `NotifyListingRejectedAsync()` - Thông báo listing rejected
  - `NotifyPackageExpiringAsync()` - Thông báo package sắp hết hạn

### 2. **Frontend Components**

#### `wwwroot/js/signalr-notifications.js`
- SignalR JavaScript client
- Auto-reconnect khi mất kết nối
- Xử lý incoming notifications
- Toast notifications
- Browser notifications (nếu user cho phép)
- Notification badge counter

#### `wwwroot/css/signalr-notifications.css`
- Styling cho toast notifications
- Notification dropdown
- Badge styling
- Animation effects

#### `Pages/Shared/_NotificationBell.cshtml`
- Partial view cho notification bell icon
- Dropdown menu hiển thị notifications
- Badge hiển thị số lượng notifications chưa đọc

### 3. **Configuration**

#### `Program.cs`
```csharp
// Đã thêm:
builder.Services.AddSignalR();
builder.Services.AddSingleton<ISignalRNotificationService, SignalRNotificationService>();

app.MapHub<NotificationHub>("/notificationHub");
```

#### `Pages/Shared/_Layout.cshtml`
- Include SignalR CDN script
- Include notification CSS và JS
- Thêm notification bell vào navbar
- Add "authenticated" class vào body

## 🚀 Cách Sử Dụng

### Backend - Gửi Notification

#### Từ Controller/Page Model:
```csharp
public class MyPageModel : PageModel
{
    private readonly ISignalRNotificationService _signalRService;
    
    public MyPageModel(ISignalRNotificationService signalRService)
    {
        _signalRService = signalRService;
    }
    
    public async Task<IActionResult> OnPostAsync()
    {
        // Gửi đến 1 user
        await _signalRService.SendToUserAsync(
            userId: Guid.Parse("..."),
            title: "Hello!",
            message: "You have a new notification",
            type: "success",
            url: "/some-page"
        );
        
        // Gửi đến role
        await _signalRService.SendToRoleAsync(
            role: "Admin",
            title: "Admin Alert",
            message: "Something needs attention",
            type: "warning"
        );
        
        // Broadcast to all
        await _signalRService.SendToAllAsync(
            title: "System Maintenance",
            message: "Scheduled maintenance in 10 minutes",
            type: "info"
        );
        
        return Page();
    }
}
```

#### Sử dụng helper methods:
```csharp
// New lead notification
await _signalRService.NotifyNewLeadAsync(
    listerId: listing.ListerId,
    propertyTitle: listing.Title,
    seekerName: seeker.DisplayName
);

// Listing approved
await _signalRService.NotifyListingApprovedAsync(
    listerId: listing.ListerId,
    propertyTitle: listing.Title
);

// Listing rejected
await _signalRService.NotifyListingRejectedAsync(
    listerId: listing.ListerId,
    propertyTitle: listing.Title,
    reason: "Missing required information"
);

// Package expiring
await _signalRService.NotifyPackageExpiringAsync(
    userId: user.Id,
    packageName: "Premium Package",
    daysRemaining: 3
);
```

### Frontend - Custom Notifications

#### Gọi từ JavaScript:
```javascript
// Notification sẽ tự động hiện khi backend gửi
// Nhưng bạn cũng có thể show toast manually:
showToast("Custom message", "success", "/optional-url");

// Mark notification as read
await markNotificationAsRead(notificationId);

// Clear all notifications
clearAllNotifications();
```

## 📋 Notification Types

- `success` ✅ - Green toast
- `error` ❌ - Red toast  
- `warning` ⚠️ - Yellow toast
- `info` ℹ️ - Blue toast

## 🔔 Features

### 1. **Toast Notifications**
- Auto-show khi có notification mới
- Auto-hide sau 5 giây
- Click vào link để navigate
- Smooth animations

### 2. **Notification Bell**
- Badge hiển thị số notifications chưa đọc
- Dropdown menu với danh sách notifications
- Click để xem chi tiết
- "Clear all" button

### 3. **Browser Notifications**
- Tự động request permission khi user login
- Desktop notifications khi có thông báo mới
- Click notification để navigate đến trang

### 4. **Auto-Reconnect**
- Tự động reconnect khi mất kết nối
- Toast thông báo khi reconnected
- Connection status indicator

### 5. **User Groups**
- Mỗi user được add vào group `User_{userId}`
- Users được add vào group `Role_{roleName}`
- Cho phép targeted notifications

## 🎯 Use Cases Được Implement

### 1. **New Lead Notification** (Đã sẵn sàng để integrate)
```csharp
// Trong LeadService.CreateLeadAsync()
await _signalRService.NotifyNewLeadAsync(
    listing.ListerId,
    listing.Title,
    seeker.DisplayName
);
```

### 2. **Listing Status Changes** (Cần integrate)
```csharp
// Trong ListingService khi approve/reject
await _signalRService.NotifyListingApprovedAsync(listerId, title);
await _signalRService.NotifyListingRejectedAsync(listerId, title, reason);
```

### 3. **Package Expiration Warnings** (Cần integrate)
```csharp
// Trong PackageExpirationService
await _signalRService.NotifyPackageExpiringAsync(userId, packageName, days);
```

### 4. **Payment Confirmations** (Cần integrate)
```csharp
await _signalRService.SendToUserAsync(
    userId,
    "Payment Successful!",
    $"Your payment of ${amount} was processed successfully",
    "success",
    "/Payment/MyTransactions"
);
```

## 🛠️ Customization

### Thay đổi notification sound:
```javascript
// Trong signalr-notifications.js
function playNotificationSound() {
    const audio = new Audio('/sounds/your-custom-sound.mp3');
    audio.volume = 0.5; // Điều chỉnh volume
    audio.play();
}
```

### Thay đổi auto-hide duration:
```javascript
// Trong showToast() function
setTimeout(() => {
    // Thay đổi 5000 thành số milliseconds bạn muốn
}, 5000);
```

### Hide connection status indicator:
```html
<!-- Xóa dòng này trong _NotificationBell.cshtml -->
<span id="signalr-status" class="badge bg-secondary">Connecting...</span>
```

## 📱 Mobile Responsive

- Toast notifications fit màn hình mobile
- Notification dropdown responsive
- Touch-friendly UI

## ⚡ Performance

- SignalR connection chỉ khởi tạo khi user đã login
- Auto-reconnect với exponential backoff
- Efficient group-based targeting
- Lightweight notifications (JSON only)

## 🔒 Security

- Hub yêu cầu `[Authorize]` attribute
- Users chỉ được add vào groups của chính họ
- Role-based access control
- No sensitive data in notifications

## 🚦 Testing

### Test locally:
1. Login với 2 accounts khác nhau (2 browsers)
2. Từ backend, gửi notification đến user
3. Verify toast hiển thị
4. Check notification bell badge
5. Test dropdown menu

### Test với DevTools:
```javascript
// Console
showToast("Test message", "success", "/test");
```

## 📊 Next Steps (Optional Enhancements)

1. ✅ Persist notifications trong database
2. ✅ Mark as read functionality  
3. ✅ Notification history page
4. ✅ Email + SignalR notifications
5. ✅ Push notifications (PWA)
6. ✅ Sound customization per user
7. ✅ Do Not Disturb mode

---

## 💡 Tips

- Test SignalR connection trong Network tab (DevTools)
- Check Console cho SignalR logs
- SignalR endpoint: `https://localhost:7068/notificationHub`
- Test với multiple browser tabs để verify group messaging

🎉 **SignalR đã sẵn sàng để sử dụng!**
