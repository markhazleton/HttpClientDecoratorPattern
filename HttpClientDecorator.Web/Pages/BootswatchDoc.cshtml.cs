using WebSpark.Bootswatch.Model;

namespace HttpClientDecorator.Web.Pages;

public class BootswatchDocModel : PageModel
{
    private readonly IStyleProvider _styleProvider;

    public BootswatchDocModel(IStyleProvider styleProvider)
    {
        _styleProvider = styleProvider;
    }

    public async Task OnGetAsync()
    {
        // Get all available styles from WebSpark.Bootswatch
        var styles = await _styleProvider.GetAsync();
        ViewData["AvailableStyles"] = styles;
    }
}