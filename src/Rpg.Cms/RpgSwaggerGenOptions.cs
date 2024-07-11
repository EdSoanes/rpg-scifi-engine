using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Rpg.ModObjects.Reflection;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;

namespace Rpg.Cms
{
    // this schema ID handler extends the Umbraco schema IDs
    // to all types in the UmbracoDocs.Samples namespace
    public class RpgSchemaIdHandler : SchemaIdHandler
    {
        public override bool CanHandle(Type type)
            => MetaSystems.IsMetaSystemType(type);

        public override string Handle(Type type)
            => MetaSystems.MetaSystemTypeName(type);
    }

    public class RpgBackOfficeSecurityRequirementsOperationFilter : BackOfficeSecurityRequirementsOperationFilterBase
    {
        protected override string ApiName => "rpg";
    }

    public class RpgSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        public void Configure(SwaggerGenOptions options)
        {
            options.SwaggerDoc("rpg", new OpenApiInfo { Title = "Rpg API v1", Version = "1.0" });
            options.OperationFilter<RpgBackOfficeSecurityRequirementsOperationFilter>();
            options.SelectSubTypesUsing(baseType =>
            {
                return MetaSystems.IsMetaSystemType(baseType)
                    ? RpgReflection.ScanForSubTypes(baseType)
                    : Array.Empty<Type>();
            });
        }
    }
}
