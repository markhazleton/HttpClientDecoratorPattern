using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HttpClientDecorator.Web.Pages;


public class JokeModel : PageModel
{
    private readonly ILogger<ListModel> _logger;
    private readonly IHttpGetCallService _service;
    public HttpGetCallResults jokeResult { get; set; } = default!;
    public Joke theJoke { get; set; } = default;
    public JokeModel(ILogger<ListModel> logger, IHttpGetCallService getCallService)
    {
        _logger = logger;
        _service = getCallService;
    }

    public async Task OnGet()
    {
        jokeResult = new HttpGetCallResults();
        jokeResult.GetPath = "https://v2.jokeapi.dev/joke/Any?safe-mode";
        jokeResult = await _service.GetAsync<Joke>(jokeResult);
        theJoke = (Joke)jokeResult?.GetResults;
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
        public string category { get; set; }
        public string type { get; set; }
        public string setup { get; set; }
        public string delivery { get; set; }
        public string joke { get; set; }
        public Flags flags { get; set; }
        public int id { get; set; }
        public bool safe { get; set; }
        public string lang { get; set; }
    }





}
