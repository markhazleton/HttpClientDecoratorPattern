using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Web.Helpers;
using HttpClientDecorator.Web.Models.Trivia;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HttpClientDecorator.Web.Pages;

public class TriviaModel : PageModel
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<TriviaModel> _logger;
    private readonly IHttpGetCallService _service;
    private TriviaMatch sessionMatch;

    public TriviaModel(ILogger<TriviaModel> logger, IHttpGetCallService getCallService, IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _service = getCallService;
        _httpContextAccessor = httpContextAccessor;

    }

    private TriviaMatch TriviaMatch
    {
        get
        {
            sessionMatch ??= _httpContextAccessor.HttpContext.Session.GetObjectFromJson<TriviaMatch>("TriviaMatch");
            sessionMatch ??= new TriviaMatch();
            return sessionMatch;
        }
        set
        {
            sessionMatch = value;
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson<TriviaMatch>("TriviaMatch", sessionMatch);
        }
    }

    public async Task OnGet(CancellationToken ct = default)
    {
        if (TriviaMatch.TriviaQuestions.Count == 0 || TriviaMatch.IsMatchFinished())
        {
            await TriviaMatch.LoadTriviaQuestions(_service, 2, ct);
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson<TriviaMatch>("TriviaMatch", TriviaMatch);
        }

        theTrivia = TriviaMatch.GetRandomTrivia()??new Trivia() {};

    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            theTrivia = TriviaMatch.TriviaQuestions.FirstOrDefault(w => w.triviaID == theTrivia?.triviaID);
            theAnswer = new TriviaAnswer(theTrivia, theAnswer);
            TriviaMatch.TriviaAnswers.Add(theAnswer);
            _httpContextAccessor.HttpContext.Session.SetObjectAsJson<TriviaMatch>("TriviaMatch", TriviaMatch);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Trivia Post");
            return Page();

        }
        return Page();
    }

    [BindProperty]
    public Trivia theTrivia { get; set; } = new Trivia();

    [BindProperty]
    public TriviaAnswer theAnswer { get; set; } = new TriviaAnswer();
    
    [BindProperty]
    public bool IsMatchFinished
    {
        get
        {
            return TriviaMatch.IsMatchFinished();
        }
    }
    [BindProperty]
    public string theMatchStatus
    {
        get
        {
            return TriviaMatch.GetMatchStatus();
        }
    }

}



