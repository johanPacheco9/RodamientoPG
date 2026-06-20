using System.Security.Claims;
using System.Security.Cryptography;
using Domain.Generics;
using Domain.Models;
using Infrastructure.AppDbContext;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
namespace Infrastructure.Services.Login
{
    public class LoginService(IHttpContextAccessor httpContextAccessor, MainDataContext dbContext, IConfiguration configuration)
    {
        // para usar dapper

        private readonly MainDataContext _dbContext = dbContext;

        public string GetConnection()
        {
            return configuration.GetConnectionString("DefaultConnection");
        }


        public async Task<(bool Success, string Message)> UserRegistrationAsync(Usuario usuarios)
        {
            try
            {
                Usuario newUser = new();
                newUser.UserName = usuarios.UserName;
                newUser.Nombre = usuarios.Nombre;
                newUser.Password = PasswordHash(usuarios.Password);
                _dbContext.Usuarios?.Add(newUser);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
               throw new Exception(ex.Message);
            }

            return (true, string.Empty);
        }



        private string PasswordHash(string password)
        {
            byte[] salt = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(salt);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 1000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);
            return Convert.ToBase64String(hashBytes);
        }

        public bool ValidatePasswordHash(string password, string dbPassword)
        {
            byte[] dbPasswordHashBytes = Convert.FromBase64String(dbPassword);

            byte[] salt = new byte[16];
            Array.Copy(dbPasswordHashBytes, 0, salt, 0, 16);

            var userPasswordBytes = new Rfc2898DeriveBytes(password, salt, 1000);
            byte[] userPasswordHash = userPasswordBytes.GetBytes(20);

            for (int i = 0; i < 20; i++)
            {
                if (dbPasswordHashBytes[i + 16] != userPasswordHash[i])
                {
                    return false;
                }
            }
            return true;
        }

        public async Task<int> UserLoginAsync(string username, string pass)
        {
            // var connectionString = this.GetConnection();
            try
            {
                Usuario? user = await _dbContext.Usuarios?.Where(c => c.UserName.ToLower() == username).FirstOrDefaultAsync()!;
                
                if (user == null)
                { 
                   throw new Exception("Usuario no encontrado");
                }
                if (pass == null)
                {
                    throw new Exception("Debe ingresar la contraseña");
                }
                if (!ValidatePasswordHash(pass, user.Password))
                {
                    throw new Exception("Usuario o contraseña incorrectos");
                    return 0;
                }
                var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.Nombre),
                        new Claim(ClaimTypes.Role, user.Role.GetDisplayName()),
                        new Claim("UserId", user.Id.ToString()),
                    };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                    IsPersistent = true,
                    // Otras propiedades...
                };
                if (httpContextAccessor.HttpContext != null)
                {
                    await httpContextAccessor.HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
            return 1;
        }
    }
}
