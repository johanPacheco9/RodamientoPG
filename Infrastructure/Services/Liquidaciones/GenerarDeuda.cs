namespace Infrastructure.Services.Liquidaciones;

public partial class LiquidacionService
{
    public async Task<int> GeneraDeuda(string pplaca, int pdesde, int phasta)
    {
        try
        {
            return await GenerarCarteraVehiculoAsync(pplaca, pdesde, phasta);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error en GenerarCarteraVehiculoAsync: {ex.Message}");

            return 0;
        }
    }
}