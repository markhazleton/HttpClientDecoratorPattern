
using HttpClientDecorator.Interfaces;
using HttpClientDecorator.Models;

namespace MauiDecoratorPattern
{
    public partial class MainPage : ContentPage
    {
        private int count = 0;
        private IHttpClientService _service;

        public MainPage(IHttpClientService service)
        {
            _service = service;
            InitializeComponent();
        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            count++;

            var JokeResult = new HttpClientSendRequest<JokeModel>
            {
                RequestPath = "https://v2.jokeapi.dev/joke/Any?safe-mode"
            };

            var ct = new CancellationToken();
            JokeResult = Task.Run(async () =>
            {
                return await _service.HttpClientSendAsync(JokeResult, ct).ConfigureAwait(true);
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
    public class JokeModel
    {
        public bool error { get; set; }
        public string? category { get; set; }
        public string? type { get; set; }
        public string? setup { get; set; }
        public string? delivery { get; set; }
        public string? joke { get; set; }
        public FlagsModel? flags { get; set; }
        public int id { get; set; }
        public bool safe { get; set; }
        public string? lang { get; set; }
    }
    public class FlagsModel
    {
        public bool nsfw { get; set; }
        public bool religious { get; set; }
        public bool political { get; set; }
        public bool racist { get; set; }
        public bool sexist { get; set; }
        public bool @explicit { get; set; }
    }
}