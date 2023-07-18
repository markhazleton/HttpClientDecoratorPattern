using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HttpClientDecorator
{
    public class HttpClientSendServiceCache : IHttpClientRequestService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<HttpClientSendServiceCache> _logger;
        private readonly IHttpClientRequestService _service;

        public HttpClientSendServiceCache(IHttpClientRequestService service, ILogger<HttpClientSendServiceCache> logger, IMemoryCache cache)
        {
            _service = service;
            _cache = cache;
            _logger = logger;
        }

        public async Task<HttpClientRequest<T>> HttpClientSendAsync<T>(HttpClientRequest<T> statusCall, CancellationToken ct)
        {
            var cacheKey = statusCall.RequestPath;

            // Try to get the cached result for the given cache key
            if (_cache.TryGetValue(cacheKey, out HttpClientRequest<T> cachedResult))
            {
                // If the result is found in the cache, return it directly
                _logger.LogInformation($"Cache hit for {cacheKey}");
                return cachedResult;
            }
            else
            {
                // If the result is not cached, make the actual HTTP request using the wrapped service
                // and store the result in the cache before returning it
                statusCall = await _service.HttpClientSendAsync(statusCall, ct).ConfigureAwait(false);
                statusCall.CompletionDate = DateTime.Now;
                _cache.Set(cacheKey, statusCall, TimeSpan.FromMinutes(statusCall.CacheDurationMinutes)); // Cache the result for 500 minutes
                _logger.LogInformation($"Cache miss for {cacheKey}");
                return statusCall;
            }
        }
    }
}
