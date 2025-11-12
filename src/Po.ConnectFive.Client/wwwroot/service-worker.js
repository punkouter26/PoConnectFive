// Service Worker for PoConnectFive PWA
// Provides offline support, caching, and background sync

const CACHE_NAME = 'poconnectfive-v1';
const RUNTIME_CACHE = 'poconnectfive-runtime-v1';

// Assets to cache on install
const PRECACHE_ASSETS = [
    '/',
    '/index.html',
    '/css/app.css',
    '/css/mobile.css',
    '/css/animations.css',
    '/css/accessibility.css',
    '/js/app.js',
    '/js/animations.js',
    '/js/accessibility.js',
    '/manifest.json',
    '/icon-192.png',
    '/icon-512.png',
    '/favicon.ico'
];

// Install event - cache core assets
self.addEventListener('install', (event) => {
    console.log('[ServiceWorker] Install event');

    event.waitUntil(
        caches.open(CACHE_NAME)
            .then((cache) => {
                console.log('[ServiceWorker] Precaching app assets');
                return cache.addAll(PRECACHE_ASSETS);
            })
            .then(() => self.skipWaiting())
    );
});

// Activate event - clean up old caches
self.addEventListener('activate', (event) => {
    console.log('[ServiceWorker] Activate event');

    event.waitUntil(
        caches.keys().then((cacheNames) => {
            return Promise.all(
                cacheNames
                    .filter((cacheName) => {
                        return cacheName !== CACHE_NAME && cacheName !== RUNTIME_CACHE;
                    })
                    .map((cacheName) => {
                        console.log('[ServiceWorker] Deleting old cache:', cacheName);
                        return caches.delete(cacheName);
                    })
            );
        }).then(() => self.clients.claim())
    );
});

// Fetch event - serve from cache, fallback to network
self.addEventListener('fetch', (event) => {
    const { request } = event;
    const url = new URL(request.url);

    // Skip cross-origin requests
    if (url.origin !== location.origin) {
        return;
    }

    // Skip API calls for now (handle separately)
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(fetchWithBackgroundSync(request));
        return;
    }

    // Cache-first strategy for app shell
    event.respondWith(
        caches.match(request)
            .then((cachedResponse) => {
                if (cachedResponse) {
                    return cachedResponse;
                }

                return caches.open(RUNTIME_CACHE).then((cache) => {
                    return fetch(request).then((response) => {
                        // Cache successful responses
                        if (response.status === 200) {
                            cache.put(request, response.clone());
                        }
                        return response;
                    });
                });
            })
            .catch(() => {
                // Return offline page if available
                return caches.match('/offline.html');
            })
    );
});

// Fetch with background sync for API calls
async function fetchWithBackgroundSync(request) {
    try {
        const response = await fetch(request);
        return response;
    } catch (error) {
        // If offline, queue request for background sync
        if ('sync' in self.registration) {
            // Store request in IndexedDB for later sync
            await queueRequest(request);

            // Register background sync
            await self.registration.sync.register('sync-stats');

            // Return cached response if available
            const cachedResponse = await caches.match(request);
            if (cachedResponse) {
                return cachedResponse;
            }
        }

        // Return offline response
        return new Response(
            JSON.stringify({ error: 'Offline', queued: true }),
            {
                status: 503,
                headers: { 'Content-Type': 'application/json' }
            }
        );
    }
}

// Queue request in IndexedDB
async function queueRequest(request) {
    const db = await openDatabase();
    const tx = db.transaction('pendingRequests', 'readwrite');
    const store = tx.objectStore('pendingRequests');

    const requestData = {
        url: request.url,
        method: request.method,
        headers: Object.fromEntries(request.headers.entries()),
        body: await request.clone().text(),
        timestamp: Date.now()
    };

    await store.add(requestData);
    return tx.complete;
}

// Open IndexedDB database
function openDatabase() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open('PoConnectFiveDB', 1);

        request.onerror = () => reject(request.error);
        request.onsuccess = () => resolve(request.result);

        request.onupgradeneeded = (event) => {
            const db = event.target.result;
            if (!db.objectStoreNames.contains('pendingRequests')) {
                db.createObjectStore('pendingRequests', { autoIncrement: true });
            }
        };
    });
}

// Background sync event
self.addEventListener('sync', (event) => {
    console.log('[ServiceWorker] Background sync:', event.tag);

    if (event.tag === 'sync-stats') {
        event.waitUntil(syncPendingRequests());
    }
});

// Sync pending requests
async function syncPendingRequests() {
    const db = await openDatabase();
    const tx = db.transaction('pendingRequests', 'readonly');
    const store = tx.objectStore('pendingRequests');
    const requests = await store.getAll();

    for (const requestData of requests) {
        try {
            await fetch(requestData.url, {
                method: requestData.method,
                headers: requestData.headers,
                body: requestData.body
            });

            // Remove from queue after successful sync
            const deleteTx = db.transaction('pendingRequests', 'readwrite');
            const deleteStore = deleteTx.objectStore('pendingRequests');
            await deleteStore.delete(requestData.id);

            console.log('[ServiceWorker] Synced request:', requestData.url);
        } catch (error) {
            console.error('[ServiceWorker] Failed to sync request:', error);
        }
    }
}

// Push notification event
self.addEventListener('push', (event) => {
    console.log('[ServiceWorker] Push notification received');

    const options = {
        body: event.data ? event.data.text() : 'New challenge available!',
        icon: '/icon-192.png',
        badge: '/icon-192.png',
        vibrate: [200, 100, 200],
        data: {
            dateOfArrival: Date.now(),
            primaryKey: 1
        },
        actions: [
            {
                action: 'explore',
                title: 'Play Now',
                icon: '/icon-192.png'
            },
            {
                action: 'close',
                title: 'Close',
                icon: '/icon-192.png'
            }
        ]
    };

    event.waitUntil(
        self.registration.showNotification('PoConnectFive', options)
    );
});

// Notification click event
self.addEventListener('notificationclick', (event) => {
    console.log('[ServiceWorker] Notification clicked:', event.action);

    event.notification.close();

    if (event.action === 'explore') {
        event.waitUntil(
            clients.openWindow('/')
        );
    }
});

// Message event - for communication with app
self.addEventListener('message', (event) => {
    console.log('[ServiceWorker] Message received:', event.data);

    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }

    if (event.data && event.data.type === 'CACHE_UPDATE') {
        event.waitUntil(updateCache());
    }
});

// Update cache with fresh content
async function updateCache() {
    const cache = await caches.open(CACHE_NAME);
    await cache.addAll(PRECACHE_ASSETS);
    console.log('[ServiceWorker] Cache updated');
}

// Periodic background sync (if supported)
self.addEventListener('periodicsync', (event) => {
    if (event.tag === 'update-leaderboard') {
        event.waitUntil(updateLeaderboardCache());
    }
});

// Update leaderboard cache in background
async function updateLeaderboardCache() {
    try {
        const difficulties = ['Easy', 'Medium', 'Hard'];
        const cache = await caches.open(RUNTIME_CACHE);

        for (const difficulty of difficulties) {
            const response = await fetch(`/api/leaderboard/${difficulty}`);
            if (response.ok) {
                await cache.put(`/api/leaderboard/${difficulty}`, response.clone());
            }
        }

        console.log('[ServiceWorker] Leaderboard cache updated');
    } catch (error) {
        console.error('[ServiceWorker] Failed to update leaderboard cache:', error);
    }
}

console.log('[ServiceWorker] Loaded and ready');
