using Rpg.Cms.Services;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddDeliveryApi()
    .AddComposers()
    .Build();

builder.Services
    .AddTransient<IRpgSystemSyncService, RpgSystemSyncService>()
    .AddTransient<IRpgPropertyTypeFactory, RpgPropertyTypeFactory>()
    .AddTransient<IRpgDocTypeFactory, RpgDocTypeFactory>()
    .AddTransient<IRpgDataTypeFactory, RpgDataTypeFactory>();

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
