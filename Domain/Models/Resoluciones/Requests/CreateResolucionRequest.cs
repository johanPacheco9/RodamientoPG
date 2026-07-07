namespace Domain.Models.Resoluciones.Requests;

public record CreateResolucionRequest(
    TipoResolucion Tipo,
    int VehiculoId,
    List<int> Vigencias, // 🚀 Cambiado: Ahora es la lista exacta de años seleccionados
    string Observaciones,
    int UsuarioId
);
