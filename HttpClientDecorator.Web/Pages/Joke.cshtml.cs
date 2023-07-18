using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HttpClientDecorator.Web.Pages;


public class JokePageModel : PageModel
{
    private readonly ILogger<JokePageModel> _logger;
    private readonly IHttpClientRequestService _service;
    public HttpClientRequest<JokeModel> JokeResult { get; set; } = default!;
    public JokeModel TheJoke { get; set; } = new JokeModel();
    public JokePageModel(ILogger<JokePageModel> logger, IHttpClientRequestService getCallService)
    {
        _logger = logger;
        _service = getCallService;
    }

    /// <summary>
    /// This method retrieves a random joke from the Joke API
    /// </summary>
    public async Task OnGet(CancellationToken ct = default)
    {
        JokeResult = new HttpClientRequest<JokeModel>();

        if (JokeResult == null)
        {
            _logger.LogError("artResponse is null");
            throw new Exception("artResponse is null");
        }

        JokeResult.RequestPath = "https://v2.jokeapi.dev/joke/Any?safe-mode";
        JokeResult = await _service.HttpClientSendAsync(JokeResult, ct).ConfigureAwait(false);

        if (_service == null)
        {
            _logger.LogError("_service is null");
            throw new NullReferenceException(nameof(_service));
        }

        if (JokeResult?.ResponseResults is null)
        {
            _logger.LogError("jokeResult.ResponseResults is null");
            TheJoke = new JokeModel()
            {
                error = true
            };
        }
        else
        {
            _logger.LogInformation("Good Response from Joke API");
            TheJoke = JokeResult.ResponseResults;
        }
    }
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

