using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.OutputCaching;

namespace CachingDemo.Policies
{
    public class CategoriaOutputCachePolicy : IOutputCachePolicy
    {
        public static readonly CategoriaOutputCachePolicy Instance = new();

        public ValueTask CacheRequestAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            var request = context.HttpContext.Request;

            // Habilita cache apenas para requisições GET ou HEAD
            if (request.Method != "GET" && request.Method != "HEAD")
            {
                context.EnableOutputCaching = false;
                return ValueTask.CompletedTask;
            }

            context.EnableOutputCaching = true;
            context.AllowCacheLookup = true;
            context.AllowCacheStorage = true;
            context.AllowLocking = true;

            if (context.HttpContext.Request.RouteValues.TryGetValue("categoria", out var catObj) && catObj != null)
            {
                string categoria = catObj.ToString()?.ToLowerInvariant() ?? "geral";
                context.Tags.Add($"categoria-{categoria}");
            }

            return ValueTask.CompletedTask;
        }

        public ValueTask ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            return ValueTask.CompletedTask;
        }

        public ValueTask ServeResponseAsync(OutputCacheContext context, CancellationToken cancellation)
        {
            return ValueTask.CompletedTask;
        }
    }
}
