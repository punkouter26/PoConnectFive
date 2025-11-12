// Service Worker for PoConnectFive PWA (Production Build)
// This file is published with the application

// Update cache version when deploying new versions
const CACHE_VERSION = 'v1.0.0';
const CACHE_NAME = `poconnectfive-${CACHE_VERSION}`;
const RUNTIME_CACHE = `poconnectfive-runtime-${CACHE_VERSION}`;

// Assets are populated during publish - Blazor will inject the list
const PRECACHE_ASSETS = self.assetsManifest || [];

// Install event
self.addEventListener('install', (event) => {
    console.log(`[ServiceWorker] Install ${CACHE_VERSION}`);

    event.waitUntil(
        caches.open(CACHE_NAME)
            .then((cache) => {
                console.log('[ServiceWorker] Precaching assets');
                return cache.addAll(PRECACHE_ASSETS);
            })
            .then(() => self.skipWaiting())
            .catch((error) => {
                console.error('[ServiceWorker] Precache failed:', error);
            })
    );
});

// Activate event
self.addEventListener('activate', (event) => {
    console.log(`[ServiceWorker] Activate ${CACHE_VERSION}`);

    event.waitUntil(
        caches.keys()
            .then((cacheNames) => {
                return Promise.all(
                    cacheNames
                        .filter((cacheName) => {
                            return cacheName.startsWith('poconnectfive-') &&
                                cacheName !== CACHE_NAME &&
                                cacheName !== RUNTIME_CACHE;
                        })
                        .map((cacheName) => {
                            console.log('[ServiceWorker] Deleting old cache:', cacheName);
                            return caches.delete(cacheName);
                        })
                );
            })
            .then(() => self.clients.claim())
    );
});

// Fetch event - optimized for production
self.addEventListener('fetch', (event) => {
    const { request } = event;
    const url = new URL(request.url);

    // Skip cross-origin requests
    if (url.origin !== location.origin) {
        return;
    }

    // Network-first for API calls
    if (url.pathname.startsWith('/api/')) {
        event.respondWith(networkFirstStrategy(request));
        return;
    }

    // Cache-first for static assets
    event.respondWith(cacheFirstStrategy(request));
});

// Cache-first strategy
async function cacheFirstStrategy(request) {
    const cachedResponse = await caches.match(request);
    if (cachedResponse) {
        return cachedResponse;
    }

    try {
        const networkResponse = await fetch(request);
        if (networkResponse.ok) {
            const cache = await caches.open(RUNTIME_CACHE);
            cache.put(request, networkResponse.clone());
        }
        return networkResponse;
    } catch (error) {
        // Return offline fallback
        return caches.match('/offline.html') || new Response('Offline', { status: 503 });
    }
}

// Network-first strategy for API calls
async function networkFirstStrategy(request) {
    try {
        const networkResponse = await fetch(request);
        if (networkResponse.ok) {
            const cache = await caches.open(RUNTIME_CACHE);
            cache.put(request, networkResponse.clone());
        }
        return networkResponse;
    } catch (error) {
        const cachedResponse = await caches.match(request);
        if (cachedResponse) {
            return cachedResponse;
        }
        return new Response(
            JSON.stringify({ error: 'Network unavailable', offline: true }),
            {
                status: 503,
                headers: { 'Content-Type': 'application/json' }
            }
        );
    }
}

console.log(`[ServiceWorker] Ready ${CACHE_VERSION}`);
