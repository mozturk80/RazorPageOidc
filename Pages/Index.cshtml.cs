using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RazorPageOidc.Pages;

[Authorize(Policy = "FinanceOnly")]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
