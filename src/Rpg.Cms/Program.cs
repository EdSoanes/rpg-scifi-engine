using Rpg.Cms;
using Rpg.Cms.Controllers.Services;
using Rpg.Cms.Services;
using Rpg.Cms.Services.Converter;
using Rpg.Cms.Services.Factories;
using Rpg.Cms.Services.Synchronizers;
using Rpg.Cyborgs;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Server;
using Umbraco.Cms.Api.Common.OpenApi;


WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

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
    .AddTransient<ContentConverter>()
    .AddRpgServer(options => options.ContentFactoryType = typeof(ContentFactory));

foreach (var propConverterType in RpgTypeScan.ForTypes<IPropConverter>())
{
    builder.Services.AddSingleton(typeof(IPropConverter), propConverterType);
}

WebApplication app = builder
    .Build();

await app.BootUmbracoAsync();

RpgTypeScan.RegisterAssembly(typeof(CyborgsSystem).Assembly);

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
