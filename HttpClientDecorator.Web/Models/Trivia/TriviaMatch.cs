using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;

namespace HttpClientDecorator.Web.Models.Trivia;

public class TriviaMatch
{
    public List<Trivia> TriviaQuestions { get; set; } = new List<Trivia>();
    public List<TriviaAnswer> TriviaAnswers { get; set; } = new List<TriviaAnswer>();

}

public static class TriviaMatchExtensions
{
    public static string GetMatchStatus(this TriviaMatch match)
    {
        return $"{match.TriviaAnswers.Select(s => s.TriviaID).Distinct().Count()} of {match.TriviaQuestions.Count} in {match.TriviaAnswers.Count} tries.";
    }
    public static bool IsMatchFinished(this TriviaMatch match)
    {
        return match.TriviaAnswers.Select(s => s.TriviaID).Distinct().Count() == match.TriviaQuestions.Count;
    }
    public static void AddQuestions(this TriviaMatch triviaMatch, List<Trivia> triviaQuestions)
    {
        triviaMatch.TriviaQuestions.AddRange(triviaQuestions);
    }
    public static void AddAnswer(this TriviaMatch triviaMatch, TriviaAnswer triviaAnswer)
    {
        triviaMatch.TriviaAnswers.Add(triviaAnswer);
    }
    public static Trivia? GetRandomTrivia(this TriviaMatch triviaMatch)
    {
        var result = triviaMatch.TriviaQuestions.Where(e => !triviaMatch.TriviaAnswers.Any(e2 => e2.TriviaID == e.triviaID)).ToList();
        var random = new Random();
        if (result.Count > 0)
        {
            var index = random.Next(result.Count());
            return result.ElementAt(index);
        }
        return null;
    }
    public static async Task LoadTriviaQuestions(this TriviaMatch triviaMatch, IHttpGetCallService _service, int questionCount = 1, CancellationToken ct = default)
    {
        int id = 1;
        if (triviaMatch.TriviaQuestions != null && triviaMatch.TriviaQuestions.Any())
        {
            id = triviaMatch.TriviaQuestions.Select(s => s.triviaID).Max() + 1;
        }
        var results = new HttpGetCallResults<OpenTBbResponse>
        {
            RequestPath = $"https://opentdb.com/api.php?amount={questionCount}&difficulty=easy&type=multiple"
        };
        results = await _service.GetAsync(results, ct);

        if (results?.ResponseResults?.results is not null)
        {
            foreach (var trivia in results.ResponseResults.results)
            {
                trivia.triviaID = id;
                id++;
                triviaMatch.TriviaQuestions.Add(trivia);
            }
        }
    }


}