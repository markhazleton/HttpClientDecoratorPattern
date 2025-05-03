using WebSpark.HttpClientUtility.Concurrent;
using WebSpark.HttpClientUtility.RequestResult;

namespace HttpClientDecorator.Web.Pages;

public class ListModel(
    IHttpRequestResultService _getCallService,
    IConfiguration _configuration) : PageModel
{
    public IList<HttpRequestResult<SiteStatus>> HttpGetCallResults { get; set; } = default!;

    public async Task OnGetAsync(CancellationToken ct = default)
    {
        string requestUrl = _configuration.GetValue<string>("RequestUrl") ?? "https://asyncdemo.azurewebsites.net/status";
        var taskProcessor = new HttpClientConcurrentProcessor(taskId => new HttpClientConcurrentModel(taskId, requestUrl), _getCallService);
        List<HttpClientConcurrentModel> results = await taskProcessor.RunAsync(10, 1, ct);
        HttpGetCallResults = results.Select(s => s.StatusCall).ToList();
    }
}
