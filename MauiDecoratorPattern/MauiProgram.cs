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

            builder.Services.AddSingleton<IHttpGetCallService>(serviceProvider =>
            {
                var logger = serviceProvider.GetRequiredService<ILogger<HttpGetCallService>>();
                var telemetryLogger = serviceProvider.GetRequiredService<ILogger<HttpGetCallServiceTelemetry>>();
                var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
                IHttpGetCallService baseService = new HttpGetCallService(logger, httpClientFactory);
                IHttpGetCallService telemetryService = new HttpGetCallServiceTelemetry(telemetryLogger, baseService);
                return telemetryService;
            });

            return builder.Build();
        }
    }
}