namespace Domain.Models;

public class Transac_Report
{
    /// <summary>
    /// Mapea el alias 'transaccion' (proviene de p.num_proc)
    /// </summary>
    public int Transaccion { get; set; }

    /// <summary>
    /// Mapea el alias 'fecha' (proviene de cast(p.fecha_proc as date))
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Mapea el alias 'No' (proviene del count(*) que cuenta cuántos registros hay en ese grupo)
    /// </summary>
    public int No { get; set; }
}