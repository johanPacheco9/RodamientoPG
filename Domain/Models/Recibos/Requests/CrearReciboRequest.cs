namespace Domain.Models.Recibos.Requests;

public record CrearReciboRequest(
    int VehiculoId,
    List<int> CarteraIdsSeleccionados
);