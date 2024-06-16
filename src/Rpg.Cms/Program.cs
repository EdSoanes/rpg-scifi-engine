using Rpg.Cms.Services;
using Rpg.Cms.Services.Factories;
using Rpg.Cms.Services.Synchronizers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

builder.Services
    .AddTransient<IRpgSystemSyncService, RpgSystemSyncService>()
    .AddTransient<IDocTypeSynchronizer, DocTypeSynchronizer>()
    .AddTransient<IDocTypeFolderSynchronizer, DocTypeFolderSynchronizer>()
    .AddTransient<IDataTypeSynchronizer, DataTypeSynchronizer>()
    .AddTransient<IDataTypeFolderSynchronizer, DataTypeFolderSynchronizer>()
    .AddTransient<DocTypeModelFactory>()
    .AddTransient<DataTypeModelFactory>();

WebApplication app = builder
    .Build();

await app.BootUmbracoAsync();


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
