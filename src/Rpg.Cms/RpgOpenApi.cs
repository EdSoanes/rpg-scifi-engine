using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Rpg.Cms.Controllers;
using Rpg.Experimental;
using System.Text.Json.Nodes;
using Umbraco.Cms.Api.Common.DependencyInjection;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Api.Management.OpenApi;

namespace Rpg.Cms
{
    /// <summary>
    /// The OpenAPI documents of the rpg api. They are served, with a user interface, from /umbraco/openapi.
    /// </summary>
    public static class RpgOpenApi
    {
        /// <summary>The character sheet api, open to character sheet apps</summary>
        public const string SheetDocument = "rpg";

        /// <summary>The backoffice api for synchronising game systems</summary>
        public const string ManagementDocument = "rpg-management";

        public static IUmbracoBuilder AddRpgOpenApi(this IUmbracoBuilder builder)
        {
            builder.Services.AddOpenApi(SheetDocument, options =>
            {
                options.AddDocumentTransformer((document, context, cancellationToken) =>
                {
                    document.Info.Title = "Rpg API";
                    document.Info.Version = "1.0";
                    document.Info.Description = "Game systems, their content libraries, and the operations a character sheet app performs on a character sheet.";
                    return Task.CompletedTask;
                });

                options.ShouldInclude = apiDescription =>
                    apiDescription.ActionDescriptor is ControllerActionDescriptor descriptor
                    && descriptor.ControllerTypeInfo.AsType() == typeof(RpgSheetController);

                //The rpg api sends enums and dice expressions as text
                options.AddSchemaTransformer((schema, context, cancellationToken) =>
                {
                    var type = Nullable.GetUnderlyingType(context.JsonTypeInfo.Type) ?? context.JsonTypeInfo.Type;

                    if (type == typeof(Dice))
                    {
                        schema.Type = JsonSchemaType.String;
                        schema.Properties?.Clear();
                        schema.Description = "A number or a dice expression, e.g. 7 or 2d6 + 1";
                    }
                    else if (type.IsEnum)
                    {
                        schema.Type = JsonSchemaType.String;
                        schema.Enum = Enum.GetNames(type).Select(x => (JsonNode)JsonValue.Create(x)!).ToList();
                    }

                    return Task.CompletedTask;
                });
            });

            builder.Services.AddOpenApiDocumentToUi(SheetDocument, "Rpg API");

            builder.AddBackOfficeOpenApiDocument(
                ManagementDocument,
                document => document
                    .WithTitle("Rpg Management API")
                    .WithBackOfficeAuthentication());

            return builder;
        }
    }
}
