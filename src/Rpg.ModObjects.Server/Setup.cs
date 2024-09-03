using Microsoft.Extensions.DependencyInjection;
using Rpg.ModObjects.Server.Services;

namespace Rpg.ModObjects.Server
{
    public static class Setup
    {
        public static IServiceCollection AddRpgServer(this IServiceCollection services, Action<RpgServerOptions> setOptions)
        {
            var options = new RpgServerOptions();
            setOptions(options);

            services
                .AddScoped<IRpgSessionlessServer, RpgSessionlessServer>()
                .AddScoped<IGraphService, GraphService>()
                .AddScoped<IActivityService, ActivityService>()
                .AddScoped(typeof(IContentFactory), options.ContentFactoryType);

            return services;
        }
    }
}
