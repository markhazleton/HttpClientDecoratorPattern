using WebSpark.HttpClientUtility.RequestResult;

namespace HttpClientDecorator.Web.Pages;

public class HttpClientUtilityDocModel : PageModel
{
    private readonly IHttpRequestResultService _httpRequestResultService;

    public HttpClientUtilityDocModel(IHttpRequestResultService httpRequestResultService)
    {
        _httpRequestResultService = httpRequestResultService;
    }

    public void OnGet()
    {
        // The page primarily displays static documentation
        // We inject the HttpRequestResultService to ensure the type is resolved correctly
        // This could be extended to include dynamic data about requests made in the app
    }
}