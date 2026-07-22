namespace Domain.Responses.Users.Enums;

public static class AppRoles
{
    public const string Administrador = nameof(Role.Administrador);
    public const string Funcionario = nameof(Role.Funcionario);
    public const string Operador = nameof(Role.Operador);
    public const string Auditor = nameof(Role.Auditor);

    public static string FromRole(Role role) => role switch
    {
        Role.Administrador => Administrador,
        Role.Funcionario => Funcionario,
        Role.Operador => Operador,
        Role.Auditor => Auditor,
        _ => string.Empty
    };
}
