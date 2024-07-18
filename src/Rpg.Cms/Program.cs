using Rpg.Cms;
using Rpg.Cms.Services;
using Rpg.Cms.Services.Converter;
using Rpg.Cms.Services.Factories;
using Rpg.Cms.Services.Synchronizers;
using Rpg.Cyborgs;
using Rpg.ModObjects.Reflection;
using Umbraco.Cms.Api.Common.OpenApi;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

//builder.Services
//    .AddCors(options => options.AddPolicy("any-origin", policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()))
//    .Configure<UmbracoPipelineOptions>(options => options.AddFilter(new UmbracoPipelineFilter("Cors", postRouting: app => app.UseCors())));

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.TypeNameHandling = TypeNameHandling.Auto;
    options.SerializerSettings.NullValueHandling = NullValueHandling.Include;
    options.SerializerSettings.Formatting = Formatting.Indented;
    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver
    {
        NamingStrategy = new CamelCaseNamingStrategy
        {
            ProcessDictionaryKeys = false,
            OverrideSpecifiedNames = true
        }
    };
});


builder.Services
    .ConfigureOptions<RpgSwaggerGenOptions>()
    .AddSingleton<ISchemaIdHandler, RpgSchemaIdHandler>();

builder.Services
    .AddTransient<ISyncTypesService, SyncTypesService>()
    .AddTransient<ISyncContentService, SyncContentService>()
    .AddTransient<SyncSessionFactory>()
    .AddTransient<IDocTypeSynchronizer, DocTypeSynchronizer>()
    .AddTransient<IDocTypeFolderSynchronizer, DocTypeFolderSynchronizer>()
    .AddTransient<IDataTypeSynchronizer, DataTypeSynchronizer>()
    .AddTransient<IDataTypeFolderSynchronizer, DataTypeFolderSynchronizer>()
    .AddTransient<DocTypeModelFactory>()
    .AddTransient<DataTypeModelFactory>()
    .AddSingleton<ContentConverter>();

foreach (var propConverterType in RpgReflection.ScanForTypes<IPropConverter>())
{
    builder.Services.AddSingleton(typeof(IPropConverter), propConverterType);
}

WebApplication app = builder
    .Build();

await app.BootUmbracoAsync();

RpgReflection.RegisterAssembly(typeof(CyborgsSystem).Assembly);

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
        
    });

await app.RunAsync();
