using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using WebSpark.HttpClientUtility.RequestResult;

namespace HttpClientDecorator.Blazor.Components.Pages;

public partial class NasaPicture
{
    [Inject]
    private ILogger<NasaPicture> Logger { get; set; } = default!;

    [Inject]
    private IHttpRequestResultService Service { get; set; } = default!;

    private bool IsLoading { get; set; } = true;
    private HttpRequestResult<NasaPictureListDto>? _apiRequest;
    private NasaPictureListDto _apiResponse = new();

    // Public properties to expose to the view
    public HttpRequestResult<NasaPictureListDto>? ApiRequest => _apiRequest;
    public NasaPictureListDto ApiResponse => _apiResponse;

    protected override async Task OnInitializedAsync()
    {
        await LoadNasaPicturesAsync();
    }

    private async Task LoadNasaPicturesAsync(CancellationToken ct = default)
    {
        IsLoading = true;
        StateHasChanged();

        try
        {
            _apiRequest = new HttpRequestResult<NasaPictureListDto>
            {
                CacheDurationMinutes = 500
            };

            if (_apiRequest == null)
            {
                Logger.LogError("nasaPictureResponse is null");
                throw new Exception("nasaPictureResponse is null");
            }

            // Use the DEMO_KEY for demonstration purposes
            // In a production environment, this should be stored in configuration or as a secret
            var apiKey = "DEMO_KEY";

            _apiRequest.RequestPath = $"https://api.nasa.gov/planetary/apod?api_key={apiKey}&count=5";
            _apiRequest = await Service.HttpSendRequestResultAsync(_apiRequest, ct: ct).ConfigureAwait(false);

            if (Service == null)
            {
                Logger.LogError("Service is null");
                throw new NullReferenceException(nameof(Service));
            }

            if (_apiRequest?.ResponseResults is null)
            {
                _apiResponse = new NasaPictureListDto();
                Logger.LogError("nasaPictureResponse.ResponseResults is null");
            }
            else
            {
                // Mask the API key in the displayed URL
                _apiRequest.RequestPath = "https://api.nasa.gov/planetary/apod?api_key=APIKEY&count=5";
                _apiResponse = _apiRequest.ResponseResults;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading NASA pictures");
            _apiResponse = new NasaPictureListDto();
        }
        finally
        {
            IsLoading = false;
            await InvokeAsync(StateHasChanged).ConfigureAwait(true);
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