using HttpClientUtility.Concurrent;
using HttpClientUtility.Models;

namespace HttpClientDecorator.Web.Pages;

public class ListModel : PageModel
{
    private readonly ILogger<ListModel> _logger;
    private readonly HttpClientUtility.Interfaces.IHttpClientService _service;
    private readonly IConfiguration _configuration;

    public ListModel(ILogger<ListModel> logger, HttpClientUtility.Interfaces.IHttpClientService getCallService, IConfiguration configuration)
    {
        _logger = logger;
        _service = getCallService;
        _configuration = configuration;
    }

    public IList<HttpClientSendRequest<SiteStatus>> HttpGetCallResults { get; set; } = default!;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        string requestUrl = _configuration.GetValue<string>("RequestUrl") ?? "https://asyncdemo.azurewebsites.net/status";
        var taskProcessor = new HttpClientConcurrentProcessor(taskId => new HttpClientConcurrentModel(taskId, requestUrl), _service);
        List<HttpClientConcurrentModel> results = await taskProcessor.RunAsync(100, 10, ct);
        HttpGetCallResults = results.Select(s => s.statusCall).ToList();
    }
}
