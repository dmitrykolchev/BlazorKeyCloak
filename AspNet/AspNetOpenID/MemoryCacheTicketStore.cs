using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace Sam.Security;

public class MemoryCacheTicketStore : ITicketStore
{
    private readonly IMemoryCache _cache;

    public MemoryCacheTicketStore()
    {
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        string key = Guid.NewGuid().ToString("N");
        Task result = RenewAsync(key, ticket);
        if (!result.IsCompleted)
        {
            // never goes here
            await result;
        }
        return key;
    }

    public Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        MemoryCacheEntryOptions options = new();
        DateTimeOffset? expiresUtc = ticket.Properties.ExpiresUtc;
        if (expiresUtc.HasValue)
        {
            options.SetAbsoluteExpiration(expiresUtc.Value);
        }
        options.SetSlidingExpiration(TimeSpan.FromHours(1));

        _cache.Set(key, ticket, options);

        return Task.CompletedTask;
    }

    public Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        _cache.TryGetValue(key, out AuthenticationTicket? ticket);
        return Task.FromResult(ticket);
    }

    public Task RemoveAsync(string key)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
