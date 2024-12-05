using Microsoft.Extensions.DependencyInjection;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Server.Services;

namespace Rpg.ModObjects.Server
{
    public static class Setup
    {
        public static IServiceCollection AddRpgServer(this IServiceCollection services, Action<RpgServerOptions> setOptions)
        {
            var options = new RpgServerOptions();
            setOptions(options);

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                var settings = RpgJson.SerializerOptions();

                options.SerializerSettings.TypeNameHandling = settings.TypeNameHandling;
                options.SerializerSettings.NullValueHandling = settings.NullValueHandling;
                options.SerializerSettings.Formatting = settings.Formatting;
                options.SerializerSettings.ContractResolver = settings.ContractResolver;

                options.SerializerSettings.Converters.Clear();
                foreach (var converter in settings.Converters)
                    options.SerializerSettings.Converters.Add(converter);
            });

            services
                .AddScoped<RpgSessionlessServer>()
                .AddScoped<GraphService>()
                .AddScoped<ActivityService>()
                .AddScoped<EntityService>()
                .AddScoped(typeof(IContentFactory), options.ContentFactoryType);

            return services;
        }
    }
}
