using HttpClientDecorator.Concurrent;

namespace HttpClientDecorator.Web.Pages;

public class ListModel : PageModel
{
    private readonly ILogger<ListModel> _logger;
    private readonly IHttpClientService _service;

    public ListModel(ILogger<ListModel> logger, IHttpClientService getCallService)
    {
        _logger = logger;
        _service = getCallService;
    }

    public IList<HttpClientSendRequest<SiteStatus>> HttpGetCallResults { get; set; } = default!;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        var taskProcessor = new HttpClientConcurrentProcessor(taskId => new HttpClientConcurrentModel(taskId, "https://asyncdemoweb.azurewebsites.net/status"), _service);
        List<HttpClientConcurrentModel> results = await taskProcessor.RunAsync(100, 10, ct);
        HttpGetCallResults = results.Select(s => s.statusCall).ToList();
    }
}
