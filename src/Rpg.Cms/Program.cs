using Rpg.Cms;
using Rpg.Cms.Controllers.Services;
using Rpg.Cms.Services;
using Rpg.Cms.Services.Converter;
using Rpg.Cms.Services.Factories;
using Rpg.Cms.Services.Synchronizers;
using Rpg.Cyborgs;
using Rpg.Experimental.Server;
using Umbraco.Cms.Core.Notifications;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

//The game systems this site knows about. Their meta data is built once, here.
var systems = new RpgSystems();
systems.Register(new CyborgsSystem());

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .AddRpgOpenApi()
    .AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, RpgSyncOnStartup>()
    .Build();

builder.Services
    .AddSingleton(systems)
    .AddScoped<IContentFactory, ContentFactory>()
    .AddScoped<RpgSessionlessServer>()
    .AddTransient<RpgSyncService>()
    .AddTransient<ISyncTypesService, SyncTypesService>()
    .AddTransient<ISyncContentService, SyncContentService>()
    .AddTransient<SyncSessionFactory>()
    .AddTransient<IDocTypeSynchronizer, DocTypeSynchronizer>()
    .AddTransient<IDocTypeFolderSynchronizer, DocTypeFolderSynchronizer>()
    .AddTransient<IDataTypeSynchronizer, DataTypeSynchronizer>()
    .AddTransient<IDataTypeFolderSynchronizer, DataTypeFolderSynchronizer>()
    .AddTransient<DocTypeModelFactory>()
    .AddTransient<DataTypeModelFactory>()
    .AddTransient<ContentConverter>();

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
