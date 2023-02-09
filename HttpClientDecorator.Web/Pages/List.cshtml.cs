using HttpClientDecorator.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HttpClientDecorator.Web.Pages;

public class ListModel : PageModel
{

    public ListModel()
    {
    }

    public IList<HttpGetCallResults> HttpGetCallResults { get; set; } = default!;

    public async Task OnGetAsync()
    {
        HttpGetCallResults = new List<HttpGetCallResults>();
    }
}
