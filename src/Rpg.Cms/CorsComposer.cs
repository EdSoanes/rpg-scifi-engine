
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Web.Common.ApplicationBuilder;

namespace Rpg.Cms
{
    public class CorsComposer : IComposer
    {
        public const string AllowAnyOriginPolicyName = nameof(AllowAnyOriginPolicyName);

        public void Compose(IUmbracoBuilder builder)
            => builder.Services
            .AddCors(options => options.AddPolicy(AllowAnyOriginPolicyName, policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()))
            .Configure<UmbracoPipelineOptions>(options => options.AddFilter(new UmbracoPipelineFilter("Cors", postRouting: app => app.UseCors(AllowAnyOriginPolicyName))));

    }
}
