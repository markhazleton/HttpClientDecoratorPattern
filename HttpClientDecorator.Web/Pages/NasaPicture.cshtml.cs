namespace HttpClientDecorator.Web.Pages;

public class NasaPicturePageModel : PageModel
{
    private readonly ILogger<NasaPicturePageModel> _logger;
    private readonly IHttpClientService _service;
    public HttpClientSendRequest<NasaPictureListDto> apiRequest { get; set; } = default!;
    public NasaPictureListDto apiResponse { get; set; } = [];
    public ArtList ArtList { get; set; } = new ArtList();
    public NasaPicturePageModel(ILogger<NasaPicturePageModel> logger, IHttpClientService getCallService)
    {
        _logger = logger;
        _service = getCallService;
    }
    public async Task OnGet(CancellationToken ct = default)
    {
        apiRequest = new HttpClientSendRequest<NasaPictureListDto>
        {
            CacheDurationMinutes = 500
        };

        if (apiRequest == null)
        {
            _logger.LogError("nasaPictureResponse is null");
            throw new Exception("nasaPictureResponse is null");
        }
        var apiKey = "DEMO_KEY";

        apiRequest.RequestPath = $"https://api.nasa.gov/planetary/apod?api_key={apiKey}&count=5";
        apiRequest = await _service.HttpClientSendAsync(apiRequest, ct).ConfigureAwait(false);

        if (_service == null)
        {
            _logger.LogError("_service is null");
            throw new NullReferenceException(nameof(_service));
        }

        if (apiRequest?.ResponseResults is null)
        {
            apiResponse = [];
            _logger.LogError("nasaPictureResponse.ResponseResults is null");
        }
        else
        {
            apiRequest.RequestPath = "https://api.nasa.gov/planetary/apod?api_key=APIKEY&count=5";
            apiResponse = apiRequest.ResponseResults;
        }
    }
}

public class NasaPictureListDto : List<NasaPictureDto> { }
public record NasaPictureDto(
    [property: JsonPropertyName("date")] string date,
    [property: JsonPropertyName("explanation")] string explanation,
    [property: JsonPropertyName("hdurl")] string hdurl,
    [property: JsonPropertyName("media_type")] string media_type,
    [property: JsonPropertyName("service_version")] string service_version,
    [property: JsonPropertyName("title")] string title,
    [property: JsonPropertyName("url")] string url,
    [property: JsonPropertyName("copyright")] string copyright
);

