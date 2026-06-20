using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Domain.Models;
using Infrastructure.Services.Login;

namespace Rodamiento.Pages.PLogin
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class RegisterModel : PageModel
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly LoginService _loginService;
        public RegisterModel(IHttpContextAccessor httpContextAccessor,
        LoginService loginService)
        {
            _httpContextAccessor = httpContextAccessor;
            _loginService = loginService;
        }

        [BindProperty]
        public string email { get; set; }

        [BindProperty]
        public string firstName { get; set; }

        [BindProperty]
        public string password { get; set; }



        public string ErrorMessage { get; set; }
        public bool IsUserRegistrationSuccessfull { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (_httpContextAccessor.HttpContext.User.Identity.IsAuthenticated)
            {
                return Redirect("/");
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Usuario usuarios = new();
            usuarios.Nombre = firstName;
            usuarios.UserName = email;
            usuarios.Password = password;
            Console.WriteLine($"paso por el onpost y la clave del usuario es:{email} y el nombre es: {firstName} y la clave es:{password}");
            var registration = await _loginService.UserRegistrationAsync(usuarios);
            if (!registration.Success)
            {
                Console.WriteLine($"paso por registration:{registration}");
            }
            else
            {
                IsUserRegistrationSuccessfull = true;
            }
            return Page();
        }
    }
}
