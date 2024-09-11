using Microsoft.Extensions.DependencyInjection;
using Rpg.ModObjects.Behaviors;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Server.Services;
using Rpg.ModObjects.States;

namespace Rpg.ModObjects.Server
{
    public static class Setup
    {
        public static IServiceCollection AddRpgServer(this IServiceCollection services, Action<RpgServerOptions> setOptions)
        {
            PolymorphicTypeResolver.Register(new DerivedTypeFactory<RpgEntity>());
            PolymorphicTypeResolver.Register(new DerivedTypeFactory<RpgContainer>());
            PolymorphicTypeResolver.Register(new DerivedTypeFactory<RpgComponent>());
            PolymorphicTypeResolver.Register(new DerivedTypeFactory<ModSet>());
            PolymorphicTypeResolver.Register(new DerivedTypeFactory<Mod>());
            PolymorphicTypeResolver.Register(new DerivedTypeFactory<State>());
            PolymorphicTypeResolver.Register(new DerivedTypeFactory<BaseBehavior>());

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
