using Microsoft.AspNetCore.Components;
using WebSpark.HttpClientUtility.RequestResult;

namespace HttpClientDecorator.Blazor.Components.Pages;

public partial class Joke
{
    [Inject]
    private ILogger<Joke> Logger { get; set; } = default!;

    [Inject]
    private IHttpRequestResultService Service { get; set; } = default!;

    private bool isLoading = true;
    private HttpRequestResult<JokeModel>? _jokeResult;
    private JokeModel? _theJoke = new();

    // Public properties to expose to the view
    public HttpRequestResult<JokeModel>? JokeResult => _jokeResult;
    public JokeModel? TheJoke => _theJoke;

    protected override async Task OnInitializedAsync()
    {
        await GetJokeAsync();
    }

    private async Task GetJokeAsync(CancellationToken ct = default)
    {
        isLoading = true;
        StateHasChanged();

        try
        {
            // Create a completely new instance to avoid any caching issues
            _jokeResult = new HttpRequestResult<JokeModel>();

            // Add a random parameter to ensure we bypass any caching
            string uniqueParam = $"nocache={DateTime.Now.Ticks}";
            _jokeResult.CacheDurationMinutes = 0; // Don't cache jokes to get a new one each time
            _jokeResult.RequestPath = $"https://v2.jokeapi.dev/joke/Any?safe-mode&{uniqueParam}";

            // Explicitly use ConfigureAwait(true) to ensure we stay on the UI thread
            _jokeResult = await Service.HttpSendRequestResultAsync(_jokeResult, ct: ct).ConfigureAwait(true);

            if (Service == null)
            {
                Logger.LogError("Service is null");
                throw new NullReferenceException(nameof(Service));
            }

            if (_jokeResult?.ResponseResults is null)
            {
                Logger.LogError("JokeResult.ResponseResults is null");
                _theJoke = new JokeModel()
                {
                    error = true
                };
            }
            else
            {
                // Create a new instance to ensure we're not stuck with a cached version
                _theJoke = new JokeModel
                {
                    error = _jokeResult.ResponseResults.error,
                    category = _jokeResult.ResponseResults.category,
                    type = _jokeResult.ResponseResults.type,
                    setup = _jokeResult.ResponseResults.setup,
                    delivery = _jokeResult.ResponseResults.delivery,
                    joke = _jokeResult.ResponseResults.joke,
                    flags = _jokeResult.ResponseResults.flags,
                    id = _jokeResult.ResponseResults.id,
                    safe = _jokeResult.ResponseResults.safe,
                    lang = _jokeResult.ResponseResults.lang
                };
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error fetching joke");
            _theJoke = new JokeModel()
            {
                error = true
            };
        }
        finally
        {
            isLoading = false;
            StateHasChanged();
        }
    }

    // Method to get a new joke when the button is clicked
    private async Task GetNewJoke()
    {
        Logger.LogInformation("GetNewJoke button clicked, fetching new joke");
        await GetJokeAsync();
    }

    public class JokeModel
    {
        public bool error { get; set; }
        public string? category { get; set; }
        public string? type { get; set; }
        public string? setup { get; set; }
        public string? delivery { get; set; }
        public string? joke { get; set; }
        public FlagsModel? flags { get; set; }
        public int id { get; set; }
        public bool safe { get; set; }
        public string? lang { get; set; }
    }

    public class FlagsModel
    {
        public bool nsfw { get; set; }
        public bool religious { get; set; }
        public bool political { get; set; }
        public bool racist { get; set; }
        public bool sexist { get; set; }
        public bool @explicit { get; set; }
    }
}