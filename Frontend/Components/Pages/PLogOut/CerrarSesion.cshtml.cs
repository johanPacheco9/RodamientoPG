using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;


namespace Procesos_app.Pages.PLogOut
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class CerrarSesionModel : PageModel
    {

        private readonly ILogger<CerrarSesionModel> _logger;
        private readonly IHttpContextAccessor _accessor; 


        public CerrarSesionModel(ILogger<CerrarSesionModel> logger, IHttpContextAccessor accessor)
        {
            _logger = logger;
            _accessor = accessor;
        }

        public async Task<RedirectResult> OnGet()
        {
            Console.WriteLine("Pasando por el onget");
            await _accessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }
    }
}