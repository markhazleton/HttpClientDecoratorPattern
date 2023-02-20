using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;
using HttpClientDecorator.Web.Models.Trivia;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HttpClientDecorator.Web.Pages;

public class TriviaModel : PageModel
{
    private readonly ILogger<TriviaModel> _logger;
    private readonly IHttpGetCallService _service;

    public TriviaModel(ILogger<TriviaModel> logger, IHttpGetCallService getCallService)
    {
        _logger = logger;
        _service = getCallService;
    }

    public async Task OnGet(CancellationToken ct = default)
    {
        triviaResult = new HttpGetCallResults<OpenTBbResponse>();

        if (triviaResult == null)
        {
            throw new ArgumentNullException(nameof(triviaResult));
        }

        triviaResult.RequestPath = "https://opentdb.com/api.php?amount=1&difficulty=easy&type=multiple";
        triviaResult = await _service.GetAsync(triviaResult, ct);

        if (_service == null)
        {
            throw new NullReferenceException(nameof(_service));
        }

        if (triviaResult?.ResponseResults is null)
        {
            theTrivia = new Trivia();
            _logger.LogError("Bad Response from Triva API");
        }
        else
        {
            theTrivia = triviaResult.ResponseResults.results.FirstOrDefault();
            _logger.LogInformation("Good Response from Triva API");
        }
    }

    public Trivia? theTrivia { get; set; } = default;
    public HttpGetCallResults<OpenTBbResponse> triviaResult { get; set; } = default!;


}



