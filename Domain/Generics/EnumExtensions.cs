using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Domain.Generics;
public static class EnumExtensions
{
    /// <summary>
    /// Obtiene el valor del Display Name de cualquier Enum.
    /// Si el atributo no existe, devuelve el nombre del enum por defecto.
    /// </summary>
    public static string GetDisplayName(this Enum value)
    {
        if (value == null) return string.Empty;

        Type type = value.GetType();
        string? name = Enum.GetName(type, value);

        if (name != null)
        {
            FieldInfo? field = type.GetField(name);
            if (field != null)
            {
                var attr = field.GetCustomAttribute<DisplayAttribute>();
                if (attr != null)
                {
                    return attr.Name ?? name;
                }
            }
        }

        return value.ToString();
    }
}