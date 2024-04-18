using Microsoft.Extensions.Caching.Memory;
using ProtoBuf;

namespace Stories.API.Services
{
    public class CacheService(IMemoryCache cache)
    {
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(30);

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> fetch)
        {
            if (!cache.TryGetValue(key, out byte[] cachedData))
            {
                var data = await fetch();
                using (var memoryStream = new MemoryStream())
                {
                    Serializer.Serialize(memoryStream, data);
                    cachedData = memoryStream.ToArray();
                }
                cache.Set(key, cachedData, _cacheExpiration);
            }

            using (var memoryStream = new MemoryStream(cachedData!))
            {
                return Serializer.Deserialize<T>(memoryStream);
            }
        }
    }
}
