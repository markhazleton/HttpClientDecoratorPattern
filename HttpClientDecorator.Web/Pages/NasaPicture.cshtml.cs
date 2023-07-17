using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json.Serialization;

namespace HttpClientDecorator.Web.Pages;

public class NasaPicturePageModel : PageModel
{
    private readonly ILogger<NasaPicturePageModel> _logger;
    private readonly IHttpClientSendService _service;
    public HttpClientSendResults<NasaPictureListDto> nasaResponse { get; set; } = default!;
    public NasaPictureListDto nasaPictureResponse { get; set; } = new NasaPictureListDto();
    public ArtList ArtList { get; set; } = new ArtList();
    public NasaPicturePageModel(ILogger<NasaPicturePageModel> logger, IHttpClientSendService getCallService)
    {
        _logger = logger;
        _service = getCallService;
    }
    public async Task OnGet(CancellationToken ct = default)
    {
        nasaResponse = new HttpClientSendResults<NasaPictureListDto>();

        if (nasaResponse == null)
        {
            _logger.LogError("nasaPictureResponse is null");
            throw new Exception("nasaPictureResponse is null");
        }
        var apiKey = "DEMO_KEY";

        nasaResponse.RequestPath = $"https://api.nasa.gov/planetary/apod?api_key={apiKey}&count=5";
        nasaResponse = await _service.HttpClientSendAsync(nasaResponse, ct).ConfigureAwait(false);

        if (_service == null)
        {
            _logger.LogError("_service is null");
            throw new NullReferenceException(nameof(_service));
        }

        if (nasaResponse?.ResponseResults is null)
        {
            nasaPictureResponse = new NasaPictureListDto();
            _logger.LogError("nasaPictureResponse.ResponseResults is null");
        }
        else
        {
            nasaResponse.RequestPath = "https://api.nasa.gov/planetary/apod?api_key=APIKEY&count=5";
            nasaResponse.CacheDurationMinutes = 500;
            nasaPictureResponse = nasaResponse.ResponseResults;
            _logger.LogInformation("Good Response from NASA API");
        }
    }
}

// Root myDeserializedClass = JsonSerializer.Deserialize<List<Root>>(myJsonResponse);
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

