using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using System.Collections.Generic;
using System.Threading;

namespace BopodaMVPPlatform.Models
{
    /// <summary>
    /// This implementation is just for demo purposes and does not scale. For better cache implementations see 
    /// https://github.com/Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2/tree/master/2-WebApp-graph-user/2-1-Call-MSGraph
    /// </summary>
    public class MSALStaticCache
    {
        private static Dictionary<string, byte[]> _staticCache = new Dictionary<string, byte[]>();

        private static ReaderWriterLockSlim _sessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly string _userId = string.Empty;
        private readonly string _cacheId = string.Empty;
        private readonly HttpContext _httpContext = null;
        private ITokenCache _cache;

        public MSALStaticCache(string userId, HttpContext httpcontext)
        {
            // not object, we want the SUB
            this._userId = userId;
            _cacheId = this._userId + "_TokenCache";
            _httpContext = httpcontext;
        }

        public ITokenCache EnablePersistence(ITokenCache cache)
        {
            this._cache = cache;
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            return cache;
        }

        public void Load(TokenCacheNotificationArgs args)
        {
            _sessionLock.EnterReadLock();
            byte[] blob = _staticCache.ContainsKey(_cacheId) ? _staticCache[_cacheId] : null;
            if (blob != null)
            {
                args.TokenCache.DeserializeMsalV3(blob);
            }
            _sessionLock.ExitReadLock();
        }

        public void Persist(TokenCacheNotificationArgs args)
        {
            _sessionLock.EnterWriteLock();

            // Reflect changes in the persistent store
            _staticCache[_cacheId] = args.TokenCache.SerializeMsalV3();
            _sessionLock.ExitWriteLock();
        }

        // Triggered right before MSAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load(args);
        }

        // Triggered right after MSAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (args.HasStateChanged)
            {
                Persist(args);
            }
        }
    }
}