using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace TelonaiWebApi.Helpers.Cache
{
    public interface ITelonaiCache
    {
        TResult Get<TResult>(string key, Func<string, TResult> sourceMethod);
    }
}
