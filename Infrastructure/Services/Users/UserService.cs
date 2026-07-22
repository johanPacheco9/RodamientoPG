using System.Security.Claims;
using Domain.Models;
using Domain.Responses.Users.Enums;
using Infrastructure.AppDbContext;
using Infrastructure.Services.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Users;

public class UserService(MainDataContext context, IHttpContextAccessor httpContextAccessor)
{
    private const string DefaultPassword = "123";

    public async Task<int> Add(Usuario usuario)
    {
        EnsureAdmin();

        var existeUsuario = await context.Usuarios!
            .AnyAsync(u => u.UserName.ToLower() == usuario.UserName.Trim().ToLower());

        if (existeUsuario)
            throw new InvalidOperationException("Ya existe un usuario con ese login.");

        usuario.UserName = usuario.UserName.Trim();
        usuario.Nombre = usuario.Nombre.Trim();
        usuario.Auth0Id = string.IsNullOrWhiteSpace(usuario.Auth0Id) ? usuario.UserName : usuario.Auth0Id.Trim();
        usuario.Password = PasswordHasher.Hash(string.IsNullOrWhiteSpace(usuario.Password) ? DefaultPassword : usuario.Password);
        usuario.IsHabilitado = true;
        usuario.UsuarioCreo = GetCurrentUserId();
        usuario.FechaCreacion = DateTime.UtcNow;

        context.Usuarios!.Add(usuario);
        return await context.SaveChangesAsync();
    }

    public async Task<int> EditUsuarios(Usuario usuario)
    {
        EnsureAdmin();

        var usuarioDb = await context.Usuarios!
            .FirstOrDefaultAsync(u => u.Id == usuario.Id);

        if (usuarioDb is null)
            throw new KeyNotFoundException("El usuario no existe.");

        usuarioDb.Nombre = usuario.Nombre.Trim();
        usuarioDb.Direccion = usuario.Direccion;
        usuarioDb.Telefono = usuario.Telefono;
        usuarioDb.Correo = usuario.Correo;
        usuarioDb.Role = usuario.Role;
        usuarioDb.Auth0Id = string.IsNullOrWhiteSpace(usuario.Auth0Id) ? usuarioDb.UserName : usuario.Auth0Id.Trim();
        usuarioDb.IsHabilitado = usuario.IsHabilitado;
        usuarioDb.UsuarioModifico = GetCurrentUserId();
        usuarioDb.FechaModificacion = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(usuario.Password))
        {
            usuarioDb.Password = PasswordHasher.Hash(usuario.Password);
        }

        return await context.SaveChangesAsync();
    }

    public async Task<int> DeleteUsuarioById(int id)
    {
        return await DarDeBaja(id);
    }

    public async Task<int> DarDeBaja(int id)
    {
        EnsureAdmin();

        return await context.Usuarios!
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.IsHabilitado, false)
                .SetProperty(u => u.UsuarioModifico, GetCurrentUserId())
                .SetProperty(u => u.FechaModificacion, DateTime.UtcNow));
    }

    public async Task<int> CambiarEstado(int id, bool habilitado)
    {
        EnsureAdmin();

        return await context.Usuarios!
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.IsHabilitado, habilitado)
                .SetProperty(u => u.UsuarioModifico, GetCurrentUserId())
                .SetProperty(u => u.FechaModificacion, DateTime.UtcNow));
    }

    public async Task<List<Usuario>> GetAll()
    {
        EnsureAdmin();

        return await context.Usuarios!
            .OrderBy(u => u.Nombre)
            .ToListAsync();
    }

    public async Task<Usuario?> GetUserById(int id)
    {
        return await context.Usuarios!.FindAsync(id);
    }

    public async Task<List<Usuario>> GetUserByName(string nombre)
    {
        EnsureAdmin();

        var filtro = nombre.Trim().ToUpper();

        return await context.Usuarios!
            .Where(u => u.Nombre.ToUpper().Contains(filtro) || u.UserName.ToUpper().Contains(filtro))
            .OrderBy(u => u.Nombre)
            .Take(20)
            .ToListAsync();
    }

    public async Task<bool> ResetKey(int id)
    {
        EnsureAdmin();

        var nuevaClave = PasswordHasher.Hash(DefaultPassword);

        var filasAfectadas = await context.Usuarios!
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.Password, nuevaClave)
                .SetProperty(u => u.UsuarioModifico, GetCurrentUserId())
                .SetProperty(u => u.FechaModificacion, DateTime.UtcNow));

        return filasAfectadas > 0;
    }

    public async Task<int> ChangeKey(int id, string nuevaKey)
    {
        var usuarioActualId = GetCurrentUserId();
        if (!IsCurrentUserAdmin() && usuarioActualId != id)
            throw new UnauthorizedAccessException("No tiene permiso para cambiar esta clave.");

        return await context.Usuarios!
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(s => s
                .SetProperty(u => u.Password, PasswordHasher.Hash(nuevaKey))
                .SetProperty(u => u.UsuarioModifico, usuarioActualId)
                .SetProperty(u => u.FechaModificacion, DateTime.UtcNow));
    }

    public bool ValidatePasswordHash(string password, string dbPasswordBase64)
    {
        return PasswordHasher.Verify(password, dbPasswordBase64);
    }

    private void EnsureAdmin()
    {
        if (!IsCurrentUserAdmin())
            throw new UnauthorizedAccessException("Solo el usuario administrador puede gestionar usuarios.");
    }

    private bool IsCurrentUserAdmin()
    {
        return httpContextAccessor.HttpContext?.User.Claims
            .Any(c => c.Type == ClaimTypes.Role && c.Value == AppRoles.Administrador) == true;
    }

    private int GetCurrentUserId()
    {
        var userId = httpContextAccessor.HttpContext?.User.FindFirst("UserId")?.Value;
        return int.TryParse(userId, out var parsed) ? parsed : 1;
    }
}
