using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json.Serialization;

namespace HttpClientDecorator.Web.Pages;

public class ListModel : PageModel
{
    private readonly object WriteLock = new();
    private readonly ILogger<ListModel> _logger;
    private readonly IHttpClientSendService _service;

    public ListModel(ILogger<ListModel> logger, IHttpClientSendService getCallService)
    {
        _logger = logger;
        _service = getCallService;
    }

    public IList<HttpClientSendResults<SiteStatus>> HttpGetCallResults { get; set; } = default!;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        var runManny = new ListRequest
        {
            MaxThreads = 100,
            IterationCount = 100,
            Endpoint = "http://52.255.84.179/status"
        };
        HttpGetCallResults = await CallEndpointMultipleTimesAsync(runManny, ct).ConfigureAwait(false);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="maxThreads"></param>
    /// <param name="itterationCount"></param>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    private async Task<List<HttpClientSendResults<SiteStatus>>> CallEndpointMultipleTimesAsync(ListRequest listRequest, CancellationToken ct)
    {
        int curIndex = 0;
        // Create a SemaphoreSlim with a maximum of maxThreads concurrent requests
        SemaphoreSlim semaphore = new(listRequest.MaxThreads);
        List<HttpClientSendResults<SiteStatus>> results = new();

        // Create a list of tasks to make the GetAsync calls
        List<Task> tasks = new();
        for (int i = 0; i < listRequest.IterationCount; i++)
        {
            // Acquire the semaphore before making the request
            await semaphore.WaitAsync().ConfigureAwait(false);
            curIndex++;
            var statusCall = new HttpClientSendResults<SiteStatus>(curIndex, listRequest.Endpoint ?? string.Empty);
            // Create a task to make the request
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    // Get The Async Results
                    var result = await _service.HttpClientSendAsync(statusCall, ct).ConfigureAwait(false);
                    lock (WriteLock)
                    {
                        results.Add(result);
                    }
                }
                finally
                {
                    // Release the semaphore
                    semaphore.Release();
                }
            }));
        }

        // Wait for all tasks to complete
        await Task.WhenAll(tasks).ConfigureAwait(false);

        // Log a message when all calls are complete
        _logger.LogInformation("All calls complete");
        return results;
    }
    public class ListRequest
    {
        public int MaxThreads { get; set; }
        public int IterationCount { get; set; }
        public string? Endpoint { get; set; }
    }

    public record BuildVersion(
        [property: JsonPropertyName("majorVersion")] int? MajorVersion,
        [property: JsonPropertyName("minorVersion")] int? MinorVersion,
        [property: JsonPropertyName("build")] int? Build,
        [property: JsonPropertyName("revision")] int? Revision
    );

    public record Features();

    public record SiteStatus(
        [property: JsonPropertyName("buildDate")] DateTime? BuildDate,
        [property: JsonPropertyName("buildVersion")] BuildVersion BuildVersion,
        [property: JsonPropertyName("features")] Features Features,
        [property: JsonPropertyName("messages")] IReadOnlyList<object> Messages,
        [property: JsonPropertyName("region")] string Region,
        [property: JsonPropertyName("status")] int? Status,
        [property: JsonPropertyName("tests")] Tests Tests
    );

    public record Tests();


}
