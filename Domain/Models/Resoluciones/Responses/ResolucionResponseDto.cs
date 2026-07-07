using Domain.Responses.Resolucion.Enums;
namespace Domain.Models.Resoluciones.Responses;

public record ResolucionResponseDto(
    int Id,
    string NumeroResolucion,
    DateTime Fecha,
    DateTime? FechaProceso,
    TipoResolucion TipoResolucion,
    decimal Valor,
    EstadoResolucion Estado,
    string? Observaciones,
    int VehiculoId,
    string PlacaVehiculo,         // 💡 Muy útil si necesitas mostrar a qué carro pertenece
    int UsuarioId,
    string NombreUsuario,         // 💡 Nombre del funcionario que la proyectó
    int? ProcesoId,
    List<int> VigenciasAfectadas, // 🚀 [2021, 2022, 2024]
    int? AnioDesde,               // 🚀 El año menor (ej: 2021) para armar rangos en UI si quieres
    int? AnioHasta                // 🚀 El año mayor (ej: 2024)
);