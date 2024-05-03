using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sigill.Common;
using Sigill.Common.OpenApi;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication(worker => worker.UseAzFunc())
    .ConfigureOpenApi()
    .ConfigureServices((hostContext, services) =>
    {
        var _configuration = hostContext.Configuration;
        services
            .AddOpenApiDocument(config =>
            {
                config.DocumentProcessors.Add(new SigillDocumentProcessor("Rpg Api", "Rpg OpenApi Schema for Api"));
                config.DocumentName = "v3";
            })
            .AddAzFunc();
    })
    .Build();

host.Run();
