// Cache only versioned app assets from Blazor's publish manifest. API data is always fetched live.
self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall()));
self.addEventListener('activate', event => event.waitUntil(onActivate()));
self.addEventListener('fetch', event => {
    if (event.request.method !== 'GET') return;

    const url = new URL(event.request.url);
    // Never return the app shell for an API URL, and never cache cross-origin responses.
    if (url.origin !== self.location.origin || /^\/api(?:\/|$)/.test(url.pathname)) return;

    event.respondWith(onFetch(event, url));
});

const cacheNamePrefix = 'ordreflow-static-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [ /\.(?:dll|wasm|html|js|json|css|woff2?|png|jpe?g|gif|ico|svg|webmanifest|blat|dat)$/ ];
const offlineAssetsExclude = [ /^service-worker(?:\.published|-assets)?\.js$/ ];
const baseUrl = new URL('./', self.location.href);
const manifestUrls = new Set(self.assetsManifest.assets.map(asset => new URL(asset.url, baseUrl).href));

async function onInstall() {
    const requests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(new URL(asset.url, baseUrl), { integrity: asset.hash, cache: 'no-cache' }));

    const cache = await caches.open(cacheName);
    await cache.addAll(requests);
}

async function onActivate() {
    const keys = await caches.keys();
    await Promise.all(keys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

async function onFetch(event, url) {
    // Static URLs in index.html carry ?v= for cache busting. Match their published
    // manifest entry without the query so those same assets also work offline.
    const assetUrl = new URL(url.pathname, url.origin).href;
    const shouldServeIndex = event.request.mode === 'navigate' && !manifestUrls.has(assetUrl);
    const cachedUrl = shouldServeIndex ? new URL('index.html', baseUrl).href
        : manifestUrls.has(assetUrl) ? assetUrl : null;

    if (cachedUrl) {
        const cache = await caches.open(cacheName);
        const response = await cache.match(cachedUrl);
        if (response) return response;
    }

    return fetch(event.request);
}
