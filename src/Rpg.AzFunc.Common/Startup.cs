using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Extensions;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Sigill.Common.Authentication;

namespace Sigill.Common
{
    public static class Startup
    {
        /// <summary>
        /// Configures ClaimsPrincipal accessor middleware
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IFunctionsWorkerApplicationBuilder UseAzFunc(this IFunctionsWorkerApplicationBuilder builder)
        {
            builder.UseMiddleware<ClaimsPrincipalMiddleware>();
            builder.ConfigureBlobStorageExtension();

            return builder;
        }

        /// <summary>
        /// Configures Application Insights logging with Serilog and adds ClaimsPrincipal accessor
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAzFunc(this IServiceCollection services)
        {
            services
                .AddHttpContextAccessor()
                .AddApplicationInsightsTelemetryWorkerService()
                .ConfigureFunctionsApplicationInsights();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Worker", LogEventLevel.Warning)
                .MinimumLevel.Override("Host", LogEventLevel.Warning)
                .MinimumLevel.Override("Function", LogEventLevel.Warning)
                .MinimumLevel.Override("Azure", LogEventLevel.Warning)
                .MinimumLevel.Override("DurableTask", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.ApplicationInsights(
                    TelemetryConfiguration.CreateDefault(),
                    TelemetryConverter.Traces,
                    LogEventLevel.Information)
                .CreateLogger();

            services
                .AddLogging(configure => configure.AddSerilog(Log.Logger))
                .AddSingleton<IClaimsPrincipalAccessor, ClaimsPrincipalAccessor>();

            services.AddAzureClients(builder =>
            {
                builder.AddBlobServiceClient<BlobServiceClient, BlobClientOptions>((options, _, sp) =>
                {
                    options.MessageEncoding = QueueMessageEncoding.Base64;
                    return new BlobServiceClient(AppSettings.StorageConnectionString, options);
                });
            });

            return services;
        }
    }
}
