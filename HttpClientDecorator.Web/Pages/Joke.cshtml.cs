using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;
using HttpClientDecorator.Models.Joke;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HttpClientDecorator.Web.Pages;


public class JokeModel : PageModel
{
    private readonly ILogger<JokeModel> _logger;
    private readonly IHttpClientSendService _service;
    public HttpClientSendResults<Joke> JokeResult { get; set; } = default!;
    public Joke TheJoke { get; set; } = new Joke();
    public JokeModel(ILogger<JokeModel> logger, IHttpClientSendService getCallService)
    {
        _logger = logger;
        _service = getCallService;
    }

    /// <summary>
    /// This method retrieves a random joke from the Joke API
    /// </summary>
    public async Task OnGet(CancellationToken ct = default)
    {
        JokeResult = new HttpClientSendResults<Joke>();

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
            TheJoke = new Joke()
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
