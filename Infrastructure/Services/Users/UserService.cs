using System.Security.Cryptography;
using Domain.Models;
using Infrastructure.AppDbContext;
using Microsoft.EntityFrameworkCore;
namespace Infrastructure.Services.Users;

public class UserService(MainDataContext context)
{
    private const int SaltSize = 16;
    private const int HashSize = 20;
    private const int Iterations = 10000;

    /// <summary>
    /// Registra un nuevo usuario aplicando un Hash seguro a la clave por defecto '123'
    /// </summary>
    public async Task<int> Add(Usuario usuario)
    {
        usuario.Password = HashPassword("123");
        usuario.UsuarioCreo = 3;

        context.Usuarios.Add(usuario);
        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Actualiza los datos de perfil de un usuario sin tocar su clave
    /// </summary>
    public async Task<int> EditUsuarios(Usuario usuario)
    {
        // 💡 Evitamos que EF Core intente sobreescribir la clave si viene vacía en el DTO de edición
        var entry = context.Entry(usuario);
        entry.State = EntityState.Modified;
        entry.Property(u => u.Password).IsModified = false; 

        return await context.SaveChangesAsync();
    }

    /// <summary>
    /// Elimina un usuario directamente por su ID usando ExecuteDelete
    /// </summary>
    public async Task<int> DeleteUsuarioById(int id)
    {
        return await context.Usuarios
            .Where(u => u.Id == id)
            .ExecuteDeleteAsync();
    }

    public async Task<List<Usuario>> GetAll()
    {
        return await context.Usuarios.ToListAsync();
    }

    public async Task<Usuario?> GetUserById(int id)
    {
        return await context.Usuarios.FindAsync(id);
    }

    /// <summary>
    /// Buscador predictivo por nombre, 100% parametrizado y protegido contra inyección SQL
    /// </summary>
    public async Task<List<Usuario>> GetUserByName(string nombre)
    {
        return await context.Usuarios
            .Where(u => u.Nombre.ToUpper().StartsWith(nombre.ToUpper()))
            .Take(20)
            .ToListAsync();
    }

    // ==========================================
    // 🔐 GESTIÓN DE CREDENCIALES (PASSWORD MGR)
    // ==========================================

    /// <summary>
    /// Restablece la clave de un usuario a la contraseña genérica '123'
    /// </summary>
    public async Task<bool> ResetKey(int id)
    {
        try
        {
            string nuevaClave = HashPassword("123");

            int filasAfectadas = await context.Usuarios
                .Where(u => u.Id == id)
                .ExecuteUpdateAsync(s => s.SetProperty(u => u.Password, nuevaClave));

            return filasAfectadas > 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en ResetKey para el ID {id}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Cambia de forma segura la clave del usuario por una nueva string suministrada
    /// </summary>
    public async Task<int> ChangeKey(int id, string nuevaKey)
    {
        string nuevaClave = HashPassword(nuevaKey);

        return await context.Usuarios
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(u => u.Password, nuevaClave));
    }

    /// <summary>
    /// Valida si una contraseña en texto plano coincide con el Hash guardado en base de datos
    /// </summary>
    public bool ValidatePasswordHash(string password, string dbPasswordBase64)
    {
        try
        {
            byte[] hashBytes = Convert.FromBase64String(dbPasswordBase64);

            byte[] salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            // 💡 Usamos la nueva API estática optimizada que no genera warnings de obsolescencia
            byte[] userHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA1, // Mantenemos SHA1 para no romper compatibilidad con hashes existentes
                HashSize);

            // Comparación en tiempo constante para mitigar ataques de temporización (Timing Attacks)
            return CryptographicOperations.FixedTimeEquals(hashBytes.AsSpan(SaltSize, HashSize), userHash);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Genera un Hash PBKDF2 robusto unido a un Salt aleatorio de 16 bytes
    /// </summary>
    private string HashPassword(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize); // API moderna de criptografía nativa (.NET 6+)

        byte[] hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            HashAlgorithmName.SHA1,
            HashSize);

        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        return Convert.ToBase64String(hashBytes);
    }
}