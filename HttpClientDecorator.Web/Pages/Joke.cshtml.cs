using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;
using HttpClientDecorator.Web.Models.Joke;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HttpClientDecorator.Web.Pages;


public class JokeModel : PageModel
{
    private readonly ILogger<JokeModel> _logger;
    private readonly IHttpGetCallService _service;
    public HttpGetCallResults<Joke> jokeResult { get; set; } = default!;
    public Joke? theJoke { get; set; } = default;
    public JokeModel(ILogger<JokeModel> logger, IHttpGetCallService getCallService)
    {
        _logger = logger;
        _service = getCallService;
    }

    /// <summary>
    /// This method retrieves a random joke from the Joke API
    /// </summary>
    public async Task OnGet(CancellationToken ct = default)
    {
        jokeResult = new HttpGetCallResults<Joke>();

        if (jokeResult == null)
        {
            _logger.LogError("jokeResult is null");
            throw new ArgumentNullException(nameof(jokeResult));
        }

        jokeResult.RequestPath = "https://v2.jokeapi.dev/joke/Any?safe-mode";
        jokeResult = await _service.GetAsync(jokeResult, ct);

        if (_service == null)
        {
            _logger.LogError("_service is null");
            throw new NullReferenceException(nameof(_service));
        }

        if (jokeResult?.ResponseResults is null)
        {
            _logger.LogError("jokeResult.ResponseResults is null");
            theJoke = new Joke()
            {
                error = true
            };
        }
        else
        {
            _logger.LogInformation("Good Response from Joke API");
            theJoke = jokeResult.ResponseResults;
        }

    }





}
