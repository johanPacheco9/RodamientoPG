using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Infrastructure.Services.Login;


namespace Rodamiento.Pages.PLogin
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class LogInModel : PageModel
    {
        private readonly LoginService _loginService;
        private readonly IHttpContextAccessor _contextAccessor;

        public LogInModel(IHttpContextAccessor httpContextAccessor, LoginService loginService)
        {
            _contextAccessor = httpContextAccessor;
            _loginService = loginService;
        }



        [BindProperty]
        public string UserName { get; set; }


        [BindProperty]
        public string pass { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPost()
        {
//            Console.WriteLine($"Usuario: {UserName}, Contrase�a: {pass}");
            var result = await _loginService.UserLoginAsync(UserName, pass);

            if (result == 1)
            {
                // Inicio de sesi�n exitoso, redirige a la p�gina Comparendos
                Console.WriteLine("�xito en el inicio de sesi�n");
                return Redirect("/Principal");
            }
            else
            {

                TempData["ErrorMessage"] = $"Error en el inicio de sesi�n: {result}";
				return Page();
            }
        }


    }
}
