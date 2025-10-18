// PWA Install Prompt and Management
let deferredPrompt = null;
let dotNetRef = null;

// Check if running on iOS
export function isIOS() {
    return /iPad|iPhone|iPod/.test(navigator.userAgent) && !window.MSStream;
}

// Check if app is already installed
export function isInstalled() {
    return window.matchMedia('(display-mode: standalone)').matches ||
        window.navigator.standalone === true;
}

// Check if we should show install prompt
export function shouldShowInstallPrompt() {
    // Don't show if already installed
    if (isInstalled()) {
        return false;
    }

    // Check visit count
    const visitCount = parseInt(localStorage.getItem('visitCount') || '0');
    const promptDismissed = localStorage.getItem('installPromptDismissed');
    const dismissDate = localStorage.getItem('installPromptDismissDate');

    // Increment visit count
    localStorage.setItem('visitCount', (visitCount + 1).toString());

    // Show after 3 visits
    if (visitCount < 2) {
        return false;
    }

    // Don't show if dismissed within last 7 days
    if (promptDismissed && dismissDate) {
        const daysSinceDismiss = (Date.now() - parseInt(dismissDate)) / (1000 * 60 * 60 * 24);
        if (daysSinceDismiss < 7) {
            return false;
        }
    }

    return true;
}

// Register for install prompt event
export function registerInstallPrompt(dotNetReference) {
    dotNetRef = dotNetReference;

    window.addEventListener('beforeinstallprompt', (e) => {
        console.log('[PWA] beforeinstallprompt event fired');
        // Prevent Chrome 67 and earlier from automatically showing the prompt
        e.preventDefault();
        // Stash the event so it can be triggered later
        deferredPrompt = e;

        // Notify Blazor component
        if (dotNetRef) {
            dotNetRef.invokeMethodAsync('ShowPrompt');
        }
    });

    // Track successful installation
    window.addEventListener('appinstalled', () => {
        console.log('[PWA] App installed successfully');
        deferredPrompt = null;

        // Track installation in analytics
        if (window.gtag) {
            gtag('event', 'pwa_install', {
                'event_category': 'engagement',
                'event_label': 'PWA Installation'
            });
        }

        // Clear install prompt state
        localStorage.removeItem('installPromptDismissed');
        localStorage.removeItem('installPromptDismissDate');
    });
}

// Trigger PWA installation
export async function installPWA() {
    if (!deferredPrompt) {
        console.log('[PWA] No deferred prompt available');
        return false;
    }

    // Show the install prompt
    deferredPrompt.prompt();

    // Wait for the user to respond to the prompt
    const { outcome } = await deferredPrompt.userChoice;
    console.log('[PWA] User response:', outcome);

    if (outcome === 'accepted') {
        console.log('[PWA] User accepted the install prompt');
    } else {
        console.log('[PWA] User dismissed the install prompt');
    }

    // Clear the deferred prompt
    deferredPrompt = null;

    return outcome === 'accepted';
}

// Dismiss install prompt
export function dismissInstallPrompt() {
    localStorage.setItem('installPromptDismissed', 'true');
    localStorage.setItem('installPromptDismissDate', Date.now().toString());
    console.log('[PWA] Install prompt dismissed');
}

// Check for service worker updates
export function checkForUpdates() {
    if ('serviceWorker' in navigator && navigator.serviceWorker.controller) {
        navigator.serviceWorker.controller.postMessage({ type: 'CACHE_UPDATE' });
    }
}

// Request persistent storage
export async function requestPersistentStorage() {
    if (navigator.storage && navigator.storage.persist) {
        const isPersisted = await navigator.storage.persist();
        console.log('[PWA] Persistent storage granted:', isPersisted);
        return isPersisted;
    }
    return false;
}

// Check storage quota
export async function checkStorageQuota() {
    if (navigator.storage && navigator.storage.estimate) {
        const estimate = await navigator.storage.estimate();
        const percentUsed = (estimate.usage / estimate.quota) * 100;
        console.log(`[PWA] Storage used: ${(estimate.usage / 1024 / 1024).toFixed(2)} MB of ${(estimate.quota / 1024 / 1024).toFixed(2)} MB (${percentUsed.toFixed(2)}%)`);
        return {
            usage: estimate.usage,
            quota: estimate.quota,
            percentUsed: percentUsed
        };
    }
    return null;
}

// Enable push notifications
export async function enablePushNotifications() {
    if (!('Notification' in window)) {
        console.log('[PWA] Notifications not supported');
        return false;
    }

    if (Notification.permission === 'granted') {
        return true;
    }

    if (Notification.permission !== 'denied') {
        const permission = await Notification.requestPermission();
        return permission === 'granted';
    }

    return false;
}

// Send push notification (local)
export function sendLocalNotification(title, options = {}) {
    if (Notification.permission === 'granted') {
        const notification = new Notification(title, {
            icon: '/icon-192.png',
            badge: '/icon-192.png',
            vibrate: [200, 100, 200],
            ...options
        });

        // Auto-close after 5 seconds
        setTimeout(() => notification.close(), 5000);

        notification.onclick = () => {
            window.focus();
            notification.close();
        };

        return true;
    }
    return false;
}

// Share API integration
export async function shareGame(title, text, url) {
    if (navigator.share) {
        try {
            await navigator.share({
                title: title,
                text: text,
                url: url || window.location.href
            });
            console.log('[PWA] Share successful');
            return true;
        } catch (error) {
            console.log('[PWA] Share cancelled or failed:', error);
            return false;
        }
    } else {
        console.log('[PWA] Web Share API not supported');
        return false;
    }
}

// Add to home screen prompt for iOS
export function showIOSInstallInstructions() {
    if (isIOS() && !isInstalled()) {
        return {
            show: true,
            message: 'To install this app on your iOS device, tap the Share button and then "Add to Home Screen".'
        };
    }
    return { show: false };
}

// Get display mode
export function getDisplayMode() {
    if (window.matchMedia('(display-mode: standalone)').matches) {
        return 'standalone';
    }
    if (window.matchMedia('(display-mode: fullscreen)').matches) {
        return 'fullscreen';
    }
    if (window.matchMedia('(display-mode: minimal-ui)').matches) {
        return 'minimal-ui';
    }
    return 'browser';
}

// Track display mode changes
window.matchMedia('(display-mode: standalone)').addEventListener('change', (e) => {
    console.log('[PWA] Display mode changed:', e.matches ? 'standalone' : 'browser');

    if (window.gtag) {
        gtag('event', 'display_mode_change', {
            'event_category': 'pwa',
            'event_label': e.matches ? 'standalone' : 'browser'
        });
    }
});

// Initialize PWA features on load
window.addEventListener('load', () => {
    console.log('[PWA] Display mode:', getDisplayMode());
    console.log('[PWA] Is installed:', isInstalled());
    console.log('[PWA] Is iOS:', isIOS());

    // Request persistent storage
    requestPersistentStorage();

    // Check storage quota
    checkStorageQuota();
});

console.log('[PWA] Module loaded');
