using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HttpClientDecorator.Web.Pages;

public class ListModel : PageModel
{
    private readonly object WriteLock = new();
    private readonly ILogger<ListModel> _logger;
    private readonly IHttpGetCallService _service;

    public ListModel(ILogger<ListModel> logger, IHttpGetCallService getCallService)
    {
        _logger = logger;
        _service = getCallService;
    }

    public IList<HttpGetCallResults<SiteStatus>> HttpGetCallResults { get; set; } = default!;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        var runManny = new ListRequest
        {
            maxThreads = 10,
            iterationCount = 100,
            endpoint = "https://asyncdemoweb.azurewebsites.net/status"
        };
        HttpGetCallResults = await CallEndpointMultipleTimesAsync(runManny, ct);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="maxThreads"></param>
    /// <param name="itterationCount"></param>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    private async Task<List<HttpGetCallResults<SiteStatus>>> CallEndpointMultipleTimesAsync(ListRequest listRequest, CancellationToken ct)
    {
        int curIndex = 0;
        // Create a SemaphoreSlim with a maximum of maxThreads concurrent requests
        SemaphoreSlim semaphore = new(listRequest.maxThreads);
        List<HttpGetCallResults<SiteStatus>> results = new();

        // Create a list of tasks to make the GetAsync calls
        List<Task> tasks = new();
        for (int i = 0; i < listRequest.iterationCount; i++)
        {
            // Acquire the semaphore before making the request
            await semaphore.WaitAsync();
            curIndex++;
            var statusCall = new HttpGetCallResults<SiteStatus>(curIndex, listRequest.endpoint);
            // Create a task to make the request
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    // Get The Async Results
                    var result = await _service.GetAsync<SiteStatus>(statusCall, ct);
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
        await Task.WhenAll(tasks);

        // Log a message when all calls are complete
        _logger.LogInformation("All calls complete");
        return results;
    }
    public class ListRequest
    {
        public int maxThreads { get; set; }
        public int iterationCount { get; set; }
        public string endpoint { get; set; }
    }

    public class BuildVersion
    {
        public int majorVersion { get; set; }
        public int minorVersion { get; set; }
        public int build { get; set; }
        public int revision { get; set; }
    }

    public class Features
    {
    }

    public class SiteStatus
    {
        public DateTime buildDate { get; set; }
        public BuildVersion buildVersion { get; set; }
        public Features features { get; set; }
        public List<object> messages { get; set; }
        public string region { get; set; }
        public int status { get; set; }
        public Tests tests { get; set; }
    }

    public class Tests
    {
    }


}
