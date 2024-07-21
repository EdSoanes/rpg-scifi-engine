using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Rpg.Cms;
using Rpg.Cms.Controllers.Services;
using Rpg.Cms.Services;
using Rpg.Cms.Services.Converter;
using Rpg.Cms.Services.Factories;
using Rpg.Cms.Services.Synchronizers;
using Rpg.Cyborgs;
using Rpg.ModObjects;
using Rpg.ModObjects.Reflection;
using Umbraco.Cms.Api.Common.OpenApi;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    var settings = RpgSerializer.JsonSerializerSettings;

    options.SerializerSettings.TypeNameHandling = settings.TypeNameHandling;
    options.SerializerSettings.NullValueHandling = settings.NullValueHandling;
    options.SerializerSettings.Formatting = settings.Formatting;
    options.SerializerSettings.ContractResolver = settings.ContractResolver;
    
    options.SerializerSettings.Converters.Clear();
    foreach (var converter in settings.Converters)
        options.SerializerSettings.Converters.Add(converter);
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
    .AddSingleton<ContentConverter>()
    .AddTransient<IContentFactory, ContentFactory>()
    .AddTransient<IGraphFactory, GraphFactory>();

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
