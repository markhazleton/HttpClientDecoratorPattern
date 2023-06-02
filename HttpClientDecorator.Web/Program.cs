using HttpClientDecorator;
using HttpClientDecorator.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

// Add the HttpGetCall and Telemetry Decorator for IHttpClientSendService interface
// Add Http Client Factory
builder.Services.AddHttpClient("HttpClientDecorator", client =>
{
    client.Timeout = TimeSpan.FromMilliseconds(3000);

    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.DefaultRequestHeaders.Add("User-Agent", "HttpClientDecorator");
    client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());
    client.DefaultRequestHeaders.Add("X-Request-Source", "HttpClientDecorator");
});

builder.Services.AddSingleton(serviceProvider =>
{
    var logger = serviceProvider.GetRequiredService<ILogger<HttpClientSendService>>();
    var telemetryLogger = serviceProvider.GetRequiredService<ILogger<HttpGetCallServiceTelemetry>>();
    var pollyLogger = serviceProvider.GetRequiredService<ILogger<HttpPollyRetryBreakerService>>();
    var httpClientFactory = serviceProvider.GetRequiredService<IHttpClientFactory>();
    IHttpClientSendService baseService = new HttpClientSendService(logger, httpClientFactory);
    IHttpClientSendService pollyService = new HttpPollyRetryBreakerService(pollyLogger, baseService);
    IHttpClientSendService telemetryService = new HttpGetCallServiceTelemetry(telemetryLogger, pollyService);
    return telemetryService;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.UseCookiePolicy();
app.UseSession();
app.MapRazorPages();
app.Run();
