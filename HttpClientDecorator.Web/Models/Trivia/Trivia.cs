namespace HttpClientDecorator.Web.Models.Trivia;

public class Trivia
{
    public int triviaID { get; set; }
    public string category { get; set; }
    public string correct_answer { get; set; }
    public string difficulty { get; set; }
    public string[] incorrect_answers { get; set; }
    public string question { get; set; }
    public string type { get; set; }
}

public class TriviaAnswer
{ 
    public int triviaID { get;private set; }
    public string answer { get;private set; }
}

public class TriviaMatch
{ 
    public List<Trivia> triviaQuestions { get;set; } = new List<Trivia>();
    public List<TriviaAnswer> triviaAnswers { get;set; } = new List<TriviaAnswer>();
}