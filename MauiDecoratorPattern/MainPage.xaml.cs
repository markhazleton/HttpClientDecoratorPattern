using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;
using HttpClientDecorator.Models.Joke;

namespace MauiDecoratorPattern
{
    public partial class MainPage : ContentPage
    {
        private int count = 0;
        private IHttpGetCallService _service;

        public MainPage(IHttpGetCallService service)
        {
            _service = service;
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            var JokeResult = new HttpGetCallResults<Joke>
            {
                RequestPath = "https://v2.jokeapi.dev/joke/Any?safe-mode"
            };

            var ct = new CancellationToken();
            JokeResult = Task.Run(async () =>
            {
                return await _service.GetAsync(JokeResult, ct).ConfigureAwait(true);
            }).Result;

            if (JokeResult.ResponseResults != null)
            {
                var TheJoke = JokeResult.ResponseResults;

                if (TheJoke.type == "single")
                {
                    JokeLbl.Text = JokeResult.ResponseResults.joke;
                }
                else if(TheJoke.type == "twopart")
                {
                    JokeLbl.Text = $"{JokeResult.ResponseResults.setup} -> {JokeResult.ResponseResults.delivery}";
                }
                else
                {
                    JokeLbl.Text = "There was an error getting the joke.";
                }
                JokeStatsLbl.Text = $"This joke took {JokeResult.ElapsedMilliseconds}MS to get from {JokeResult.RequestPath}";

            }
            SemanticScreenReader.Announce(CounterBtn.Text);
        }
    }
}