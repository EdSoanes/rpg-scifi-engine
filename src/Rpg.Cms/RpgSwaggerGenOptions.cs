using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.States;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;

namespace Rpg.Cms
{
    //public class RpgBackOfficeSecurityRequirementsOperationFilter : BackOfficeSecurityRequirementsOperationFilterBase
    //{
    //    protected override string ApiName => "rpg";

        
    //}

    public class CustomModelDocumentFilter<T> : IDocumentFilter where T : class
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            
            context.SchemaGenerator.GenerateSchema(typeof(T), context.SchemaRepository);
        }
    }

    public class MySwaggerSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null)
                return;

            var graphStateProp = context.Type.GetProperties()
                .FirstOrDefault(t => t.PropertyType == typeof(RpgGraphState));

            if (graphStateProp != null)
            {
                var property = schema.Properties.Keys
                    .SingleOrDefault(x => x.ToLower() == graphStateProp.Name.ToLower());

                if (property != null)
                    schema.Properties[property].Example = OpenApiAnyFactory.CreateFromJson("{}");
            }
        }
    }

    public class MyOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var response in operation.Responses)
            {
                if (response.Value.Headers.ContainsKey("Umb-Notifications"))
                    response.Value.Headers.Remove("Umb-Notifications");
            }
        }
    }

    public class RpgSwaggerGenOptions : IConfigureOptions<SwaggerGenOptions>
    {
        public void Configure(SwaggerGenOptions options)
        {
            options.SwaggerDoc("rpg", new OpenApiInfo { Title = "Rpg API v1", Version = "1.0" });
            options.DocumentFilter<CustomModelDocumentFilter<State>>();
            options.SchemaFilter<MySwaggerSchemaFilter>();
            options.OperationFilter<MyOperationFilter>();
            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo)) return false;
                if (methodInfo.DeclaringType == null) return false;

                var include = methodInfo.DeclaringType.Namespace == "Rpg.Cms.Controllers";
                return include;
            });
            //options.OperationFilter<RpgBackOfficeSecurityRequirementsOperationFilter>();
            options.CustomSchemaIds((type) => RpgTypeScan.OpenApiSchemaId(type));
            options.SelectSubTypesUsing(baseType =>
            {
                return MetaSystems.IsMetaSystemType(baseType)
                    ? RpgTypeScan.ForSubTypes(baseType)
                    : Array.Empty<Type>();
            });
        }
    }
}
