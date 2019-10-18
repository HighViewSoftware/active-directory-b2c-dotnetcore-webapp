using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;
using System.Threading;

namespace BopodaMVPPlatform.Models
{
    public class MSALSessionCache
    {
        private static ReaderWriterLockSlim _sessionLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        string _userId = string.Empty;
        string _cacheId = string.Empty;
        private readonly HttpContext _httpContext = null;
        private ITokenCache _cache;

        public MSALSessionCache(string userId, HttpContext httpcontext)
        {
            // not object, we want the SUB
            _userId = userId;
            _cacheId = _userId + "_TokenCache";
            _httpContext = httpcontext;
        }

        public ITokenCache EnablePersistence(ITokenCache cache)
        {
            this._cache = cache;
            cache.SetBeforeAccess(BeforeAccessNotification);
            cache.SetAfterAccess(AfterAccessNotification);
            return cache;
        }

        public void SaveUserStateValue(string state)
        {
            _sessionLock.EnterWriteLock();
            _httpContext.Session.SetString(_cacheId + "_state", state);
            _sessionLock.ExitWriteLock();
        }
        public string ReadUserStateValue()
        {
            string state = string.Empty;
            _sessionLock.EnterReadLock();
            state = (string)_httpContext.Session.GetString(_cacheId + "_state");
            _sessionLock.ExitReadLock();
            return state;
        }
        public void Load(TokenCacheNotificationArgs args)
        {
            _sessionLock.EnterReadLock();
            byte[] blob = _httpContext.Session.Get(_cacheId);
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
            _httpContext.Session.Set(_cacheId, args.TokenCache.SerializeMsalV3());
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