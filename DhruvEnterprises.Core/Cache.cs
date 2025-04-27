using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Caching;

namespace DhruvEnterprises.Core
{
    public static class Cache
    {
        internal static bool Add(string key, object value, DateTimeOffset dateTime)
        {
            return MemoryCache.Default.Add(key, value, dateTime);
        }

        public static T AddOrReplace<T>(string key, object value, DateTimeOffset dateTime)
        {
            if (MemoryCache.Default[key] == null)
                Add(key, value, dateTime);

            return (T)Convert.ChangeType(MemoryCache.Default[key], typeof(T));
        }

        public static object Get(string key)
        {
            return MemoryCache.Default[key];
        }

        public static T Get<T>(string key)
        {
            if (MemoryCache.Default[key] != null)
                return (T)Convert.ChangeType(MemoryCache.Default[key], typeof(T));
            else
                return default(T);
        }

        public static void Remove(string key)
        {
            MemoryCache.Default.Remove(key);
        }

        public static void RemoveAll()
        {
            foreach (var cache in MemoryCache.Default)
                MemoryCache.Default.Remove(cache.Key);
        }

    }
}
