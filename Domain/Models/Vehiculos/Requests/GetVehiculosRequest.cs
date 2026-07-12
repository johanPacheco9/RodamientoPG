namespace Domain.Models.Vehiculos.Requests;

public class GetVehiculosRequest 
{
    public string? Placa { get; set; }
    public int Pagina { get; set; } = 1;
    public int PorPagina { get; set; } = 20;
}

