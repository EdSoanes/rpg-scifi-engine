
//using Umbraco.Cms.Core.Composing;
//using Umbraco.Cms.Web.Common.ApplicationBuilder;

//namespace Rpg.Cms
//{
//    public class CorsComposer : IComposer
//    {
//        public const string AllowAnyOriginPolicyName = nameof(AllowAnyOriginPolicyName);

//        public void Compose(IUmbracoBuilder builder)
//            => builder.Services
//            .AddCors(options => options.AddPolicy(AllowAnyOriginPolicyName, policy => policy.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod()))
//            .Configure<UmbracoPipelineOptions>(options => options.AddFilter(new UmbracoPipelineFilter("Cors", postRouting: app => app.UseCors(AllowAnyOriginPolicyName))));
//            //// For testing only
//            //.Configure<UmbracoPipelineOptions>(options => options.AddFilter(new UmbracoPipelineFilter("CorsTest", endpoints: app => app.UseEndpoints(endpoints =>
//            //{
//            //    endpoints.MapGet("/echo", context => context.Response.WriteAsync("echo")).RequireCors(AllowAnyOriginPolicyName);
//            //    endpoints.MapGet("/echo2", context => context.Response.WriteAsync("echo2"));
//            //}))));
//    }
//}
