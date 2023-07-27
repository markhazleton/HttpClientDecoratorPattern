namespace HttpClientDecorator.Web.Pages;

public class CircuitBreakerModel : PageModel
{
    private static readonly Random random = new();
    private readonly object WriteLock = new();
    private readonly ILogger<CircuitBreakerModel> _logger;
    private readonly IHttpClientService _service;

    public CircuitBreakerModel(ILogger<CircuitBreakerModel> logger, IHttpClientService getCallService)
    {
        _logger = logger;
        _service = getCallService;
    }

    public IList<HttpClientSendRequest<SiteStatus>> HttpGetCallResults { get; set; } = default!;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        var runManny = new ListRequest
        {
            MaxThreads = 1,
            IterationCount = 10,
            Endpoint = "https://asyncdemoweb.azurewebsites.net/api/remote/Results",
            RequestMethod = HttpMethod.Post
        };
        HttpGetCallResults = await CallEndpointMultipleTimesAsync(runManny, ct);
    }

    public StringContent GetRandomSiteStatus(int curIndex)
    {
        // Create the request body object
        var requestBody = new SiteStatus
        {
            loopCount = random.Next(201),
            maxTimeMS = 500,
            runTimeMS = 0,
            message = string.Empty,
            resultValue = string.Empty
        };
        return new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json");

    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="maxThreads"></param>
    /// <param name="itterationCount"></param>
    /// <param name="endpoint"></param>
    /// <returns></returns>
    private async Task<List<HttpClientSendRequest<SiteStatus>>> CallEndpointMultipleTimesAsync(ListRequest listRequest, CancellationToken ct)
    {
        int curIndex = 0;
        // Create a SemaphoreSlim with a maximum of maxThreads concurrent requests
        SemaphoreSlim semaphore = new(listRequest.MaxThreads);
        List<HttpClientSendRequest<SiteStatus>> results = new();

        // Create a list of tasks to make the GetAsync calls
        List<Task> tasks = new();
        for (int i = 0; i < listRequest.IterationCount; i++)
        {
            // Acquire the semaphore before making the request
            await semaphore.WaitAsync(ct).ConfigureAwait(false);
            curIndex++;

            var statusCall = new HttpClientSendRequest<SiteStatus>(curIndex, listRequest.Endpoint ?? string.Empty)
            {
                CacheDurationMinutes = 0,
                RequestMethod = listRequest.RequestMethod,
                RequestBody = GetRandomSiteStatus(curIndex),
            };
            statusCall.RequestPath = $"{statusCall.RequestPath}?Index={curIndex}";


            // Create a task to make the request
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    // Get The Async Results
                    var test = await statusCall.RequestBody.ReadAsStringAsync();

                    var result = await _service.HttpClientSendAsync(statusCall, ct).ConfigureAwait(false);

                    lock (WriteLock)
                    {
                        if (result.ErrorList.Count > 0)
                        {
                            result.ErrorList.Add(test);
                        }
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
        public HttpMethod RequestMethod { get; set; } = HttpMethod.Get;
        public StringContent RequestBody { get; set; } = new(string.Empty);
    }
    public class SiteStatus
    {
        public int loopCount { get; set; }
        public int maxTimeMS { get; set; }
        public int runTimeMS { get; set; }
        public string message { get; set; }
        public string resultValue { get; set; }
    }
}
