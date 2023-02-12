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

    public IList<HttpGetCallResults<string>> HttpGetCallResults { get; set; } = default!;

    public async Task OnGetAsync()
    {
        HttpGetCallResults = await CallEndpointMultipleTimes();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="maxThreads"></param>
    /// <param name="itterationCount"></param>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    private async Task<List<HttpGetCallResults<string>>> CallEndpointMultipleTimes(int maxThreads = 5, int itterationCount = 100, string endpoint = "https://asyncdemoweb.azurewebsites.net/status")
    {
        int curIndex = 0;
        // Create a SemaphoreSlim with a maximum of maxThreads concurrent requests
        SemaphoreSlim semaphore = new(maxThreads);
        List<HttpGetCallResults<string>> results = new();

        // Create a list of tasks to make the GetAsync calls
        List<Task> tasks = new();
        for (int i = 0; i < itterationCount; i++)
        {
            // Acquire the semaphore before making the request
            await semaphore.WaitAsync();
            curIndex++;
            var statusCall = new HttpGetCallResults<string>(curIndex, endpoint);
            // Create a task to make the request
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    // Get The Async Results
                    var result = await _service.GetAsync<string>(statusCall);
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

}
