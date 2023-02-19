using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;
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
            throw new ArgumentNullException(nameof(jokeResult));
        }

        jokeResult.RequestPath = "https://v2.jokeapi.dev/joke/Any?safe-mode";
        jokeResult = await _service.GetAsync<Joke>(jokeResult, ct);

        if (_service == null)
        {
            throw new NullReferenceException(nameof(_service));
        }

        if (jokeResult?.ResponseResults is null)
        {
            theJoke = new Joke()
            {
                error = true
            };
        }
        else
        {
            theJoke = jokeResult.ResponseResults;
        }

    }
    public class Flags
    {
        public bool nsfw { get; set; }
        public bool religious { get; set; }
        public bool political { get; set; }
        public bool racist { get; set; }
        public bool sexist { get; set; }
        public bool @explicit { get; set; }
    }

    public class Joke
    {
        public bool error { get; set; }
        public string? category { get; set; }
        public string? type { get; set; }
        public string? setup { get; set; }
        public string? delivery { get; set; }
        public string? joke { get; set; }
        public Flags? flags { get; set; }
        public int id { get; set; }
        public bool safe { get; set; }
        public string? lang { get; set; }
    }





}
