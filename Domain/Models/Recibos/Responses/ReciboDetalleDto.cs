namespace Domain.Models.Recibos.Responses;

public record ReciboDetalleDto(
    int Id,
    int CarteraId,
    int Vigencia,
    string Concepto,
    decimal ValorTotal
);