// SignalR Real-time Notification System
let notificationConnection = null;
let notificationCount = 0;

// Initialize SignalR connection
async function initializeSignalR() {
    try {
        notificationConnection = new signalR.HubConnectionBuilder()
            .withUrl("/notificationHub")
            .withAutomaticReconnect()
            .configureLogging(signalR.LogLevel.Information)
            .build();

        // Handle incoming notifications
        notificationConnection.on("ReceiveNotification", (notification) => {
            handleNotification(notification);
        });

        // Handle reconnection
        notificationConnection.onreconnecting(() => {
            console.log("SignalR reconnecting...");
        });

        notificationConnection.onreconnected(() => {
            console.log("SignalR reconnected!");
            showToast("Connection restored", "info");
        });

        notificationConnection.onclose(() => {
            console.log("SignalR connection closed");
        });

        // Start connection
        await notificationConnection.start();
        console.log("SignalR connected successfully");
        
        // Update UI to show connection status
        updateConnectionStatus(true);
        
    } catch (err) {
        console.error("SignalR connection failed:", err);
        updateConnectionStatus(false);
        
        // Retry connection after 5 seconds
        setTimeout(() => initializeSignalR(), 5000);
    }
}

// Handle incoming notification
function handleNotification(notification) {
    // Play notification sound (optional)
    // playNotificationSound();
    
    // Show toast notification
    showToast(notification.message, notification.type, notification.url);
    
    // Update notification badge
    updateNotificationBadge();
    
    // Add to notification dropdown (if exists)
    addToNotificationDropdown(notification);
    
    // Show browser notification if permission granted
    if (Notification.permission === "granted") {
        showBrowserNotification(notification);
    }
}

// Show toast notification
function showToast(message, type = "info", url = null) {
    const toastContainer = document.getElementById('toast-container') || createToastContainer();
    
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-bg-${getBootstrapType(type)} border-0 show`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');
    
    const toastBody = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
                ${url ? `<br><a href="${url}" class="text-white fw-bold">View Details</a>` : ''}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    
    toast.innerHTML = toastBody;
    toastContainer.appendChild(toast);
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        const bsToast = new bootstrap.Toast(toast);
        bsToast.hide();
        setTimeout(() => toast.remove(), 500);
    }, 5000);
}

// Create toast container if doesn't exist
function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toast-container';
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = '9999';
    document.body.appendChild(container);
    return container;
}

// Get Bootstrap color class from notification type
function getBootstrapType(type) {
    const typeMap = {
        'success': 'success',
        'error': 'danger',
        'warning': 'warning',
        'info': 'info'
    };
    return typeMap[type] || 'primary';
}

// Update notification badge
function updateNotificationBadge() {
    notificationCount++;
    const badge = document.getElementById('notification-badge');
    if (badge) {
        badge.textContent = notificationCount > 99 ? '99+' : notificationCount;
        badge.classList.remove('d-none');
    }
}

// Add notification to dropdown menu
function addToNotificationDropdown(notification) {
    const dropdown = document.getElementById('notification-dropdown');
    if (!dropdown) return;
    
    const notificationItem = document.createElement('a');
    notificationItem.href = notification.url || '#';
    notificationItem.className = 'dropdown-item notification-item';
    notificationItem.innerHTML = `
        <div class="notification-content">
            <div class="notification-title">${notification.title}</div>
            <div class="notification-message text-muted">${notification.message}</div>
            <small class="text-muted">${getRelativeTime(notification.timestamp)}</small>
        </div>
    `;
    
    const emptyMessage = dropdown.querySelector('.no-notifications');
    if (emptyMessage) {
        emptyMessage.remove();
    }
    
    dropdown.insertBefore(notificationItem, dropdown.firstChild);
}

// Show browser notification
function showBrowserNotification(notification) {
    if (Notification.permission === "granted") {
        const browserNotif = new Notification(notification.title, {
            body: notification.message,
            icon: '/images/logo.png',
            badge: '/images/logo.png',
            tag: notification.id
        });
        
        browserNotif.onclick = function() {
            if (notification.url) {
                window.location.href = notification.url;
            }
            browserNotif.close();
        };
    }
}

// Request browser notification permission
function requestNotificationPermission() {
    if ('Notification' in window && Notification.permission === 'default') {
        Notification.requestPermission().then(permission => {
            if (permission === 'granted') {
                console.log('Notification permission granted');
            }
        });
    }
}

// Update connection status indicator
function updateConnectionStatus(connected) {
    const statusIndicator = document.getElementById('signalr-status');
    if (statusIndicator) {
        if (connected) {
            statusIndicator.className = 'badge bg-success';
            statusIndicator.textContent = 'Connected';
        } else {
            statusIndicator.className = 'badge bg-danger';
            statusIndicator.textContent = 'Disconnected';
        }
    }
}

// Get relative time (e.g., "2 minutes ago")
function getRelativeTime(timestamp) {
    const now = new Date();
    const time = new Date(timestamp);
    const diffMs = now - time;
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 1) return 'Just now';
    if (diffMins < 60) return `${diffMins} minute${diffMins > 1 ? 's' : ''} ago`;
    
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours < 24) return `${diffHours} hour${diffHours > 1 ? 's' : ''} ago`;
    
    const diffDays = Math.floor(diffHours / 24);
    return `${diffDays} day${diffDays > 1 ? 's' : ''} ago`;
}

// Mark notification as read
async function markNotificationAsRead(notificationId) {
    if (notificationConnection && notificationConnection.state === signalR.HubConnectionState.Connected) {
        try {
            await notificationConnection.invoke("MarkAsRead", notificationId);
        } catch (err) {
            console.error("Error marking notification as read:", err);
        }
    }
}

// Play notification sound (optional)
function playNotificationSound() {
    const audio = new Audio('/sounds/notification.mp3');
    audio.volume = 0.3;
    audio.play().catch(err => console.log('Audio play failed:', err));
}

// Initialize when DOM is ready
document.addEventListener('DOMContentLoaded', function() {
    // Check if user is authenticated before initializing SignalR
    const isAuthenticated = document.body.classList.contains('authenticated');
    
    if (isAuthenticated) {
        // Initialize SignalR
        initializeSignalR();
        
        // Request notification permission
        requestNotificationPermission();
    }
});

// Cleanup on page unload
window.addEventListener('beforeunload', function() {
    if (notificationConnection) {
        notificationConnection.stop();
    }
});
