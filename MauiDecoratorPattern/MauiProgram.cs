using HttpClientDecorator;
using HttpClientDecorator.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net;

namespace MauiDecoratorPattern
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif


            builder.Services.AddSingleton<MainPage>();


            // Add the HttpGetCall and Telemetry Decorator for IHttpGetCallService interface
            // Add Http Client Factory
            builder.Services.AddHttpClient("HttpClientDecorator", client =>
            {
                client.Timeout = TimeSpan.FromMilliseconds(1500);

                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "HttpClientDecorator");
                client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
                client.DefaultRequestHeaders.Add("X-Request-Source", "HttpClientDecorator");
            });

            builder.Services.AddSingleton<IHttpClientService>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<HttpClientSendService>>();
                var telemetryLogger = serviceProvider.GetRequiredService<ILogger<HttpClientSendServiceTelemetry>>();
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                IHttpClientService baseService = new HttpClientSendService(logger, httpClientFactory.CreateClient("Maui"));
                IHttpClientService telemetryService = new HttpClientSendServiceTelemetry(telemetryLogger, baseService);
                return telemetryService;
            });

            return builder.Build();
        }
    }
}