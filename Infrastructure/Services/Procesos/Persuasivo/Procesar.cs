namespace Infrastructure.Services.Procesos.Persuasivo;

public partial class PersuasivoService
{
    public async Task<int> Procesar(int pvigencia)
    {
        try
        {
            return await CrearProcesosPersuasivo(pvigencia);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en CrearProcesosPersuasivosAsync: {ex.Message}");
            return 0;
        }
    }
}