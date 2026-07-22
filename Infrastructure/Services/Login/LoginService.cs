using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Models;
using Domain.Responses.Users.Enums;
using Infrastructure.AppDbContext;
using Infrastructure.Services.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Services.Login;

public class LoginService(
    IHttpContextAccessor httpContextAccessor,
    MainDataContext dbContext,
    IConfiguration configuration)
{
    public string GetConnection()
    {
        return configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    }

    public async Task<(bool Success, string Message)> UserRegistrationAsync(Usuario usuarios)
    {
        var nuevoUsuario = new Usuario
        {
            UserName = usuarios.UserName.Trim(),
            Nombre = usuarios.Nombre.Trim(),
            Password = PasswordHasher.Hash(usuarios.Password),
            Role = usuarios.Role,
            Auth0Id = string.IsNullOrWhiteSpace(usuarios.Auth0Id) ? usuarios.UserName.Trim() : usuarios.Auth0Id,
            Correo = usuarios.Correo,
            Direccion = usuarios.Direccion,
            Telefono = usuarios.Telefono,
            IsHabilitado = true,
            FechaCreacion = DateTime.UtcNow,
            UsuarioCreo = 1
        };

        dbContext.Usuarios!.Add(nuevoUsuario);
        await dbContext.SaveChangesAsync();

        return (true, string.Empty);
    }

    public bool ValidatePasswordHash(string password, string dbPassword)
    {
        return PasswordHasher.Verify(password, dbPassword);
    }

    public async Task<int> UserLoginAsync(string username, string pass)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(pass))
                return 0;

            var normalizedUserName = username.Trim().ToLower();
            var user = await dbContext.Usuarios!
                .FirstOrDefaultAsync(c => c.UserName.ToLower() == normalizedUserName);

            if (user is null || !user.IsHabilitado)
                return 0;

            if (!PasswordHasher.Verify(pass, user.Password))
                return 0;

            var claims = BuildClaims(user);
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var jwt = GenerateJwt(user, claims);

            var authProperties = new AuthenticationProperties
            {
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8),
                IsPersistent = true
            };

            authProperties.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = jwt }
            });

            if (httpContextAccessor.HttpContext is not null)
            {
                await httpContextAccessor.HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
            }

            return 1;
        }
        catch
        {
            return 0;
        }
    }

    public string GenerateJwt(Usuario user, IEnumerable<Claim>? baseClaims = null)
    {
        var jwtKey = configuration["Jwt:Key"] ?? "rodamiento-dev-key-change-me-1098825894";
        var issuer = configuration["Jwt:Issuer"] ?? "Rodamiento";
        var audience = configuration["Jwt:Audience"] ?? "Rodamiento.Frontend";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: baseClaims ?? BuildClaims(user),
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static List<Claim> BuildClaims(Usuario user)
    {
        return
        [
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Nombre),
            new Claim(ClaimTypes.Role, AppRoles.FromRole(user.Role)),
            new Claim("UserId", user.Id.ToString()),
            new Claim("UserName", user.UserName)
        ];
    }
}
