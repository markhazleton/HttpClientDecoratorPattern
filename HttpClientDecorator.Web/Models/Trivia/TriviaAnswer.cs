namespace HttpClientDecorator.Web.Models.Trivia
{
    public class TriviaAnswer
    {
        public TriviaAnswer()
        {
            IsValid = false;
            ErrorMessage = "No Trivia ID specified.";
            Correct = false;
            Answer = string.Empty;
        }

        public TriviaAnswer(Trivia? theTrivia, TriviaAnswer answer)
        {
            if (theTrivia is null)
            {
                IsValid = false;
                ErrorMessage = "No Trivia specified.";
            }
            IsValid = true;
            ErrorMessage = string.Empty;
            TriviaID = theTrivia?.triviaID??-1;
            Answer = answer.Answer;
            Correct = (theTrivia?.correct_answer == answer.Answer);
        }
        public int TriviaID { get; set; }
        public string Answer { get; set; }
        public bool Correct { get; set; }
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
}
