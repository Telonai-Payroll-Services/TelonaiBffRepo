using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace TelonaiWebApi.Helpers.Cache
{
    public class TelonaiCache:ITelonaiCache
    {
        IMemoryCache _cache;
        public TimeSpan Expiration { get; set; }

        public TelonaiCache(IMemoryCache cache,IOptions<ApiOptions> options)
        {
            _cache = cache;
            Expiration=TimeSpan.FromHours(options.Value.CacheExpiration);
        }

        public TResult Get<TResult>(string key, Func<string, TResult> sourceMethod)
        {
            if (_cache.TryGetValue(key, out TResult result))
                return result;
            
            result = sourceMethod(key);
            _cache.Set(key, result, Expiration);
         
            return result;
        }
    }
}
