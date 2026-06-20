namespace Infrastructure.Services.Procesos.Coactivo;

public partial class CoactivoService
{
    public async Task<int> Procesar(int opcion, int ptran)
    {
        try
        {
            return await PersuasivoAMandamiento(opcion, ptran);
        }
        catch (Exception ex)
        {
            throw new Exception("Error procesando persuasivo", ex);
        }
    }
}