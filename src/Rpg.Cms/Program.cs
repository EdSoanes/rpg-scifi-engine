using Rpg.Cms.Services;
using Rpg.Cms.Services.Converter;
using Rpg.Cms.Services.Factories;
using Rpg.Cms.Services.Synchronizers;
using Rpg.ModObjects.Reflection;
using Rpg.Sys;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

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

RpgReflection.RegisterAssembly(typeof(MetaSystem).Assembly);

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
