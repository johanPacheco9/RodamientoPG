using Domain.Models.Resoluciones;
using Domain.Models.Resoluciones.Requests;
using Domain.Responses.Resolucion.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Resoluciones;

public partial class ResolucionService
{
    public async Task<bool> Create(CreateResolucionRequest request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Buscar las carteras que se verán afectadas (usando la lista exacta de vigencias)
            var carterasAfectadas = await _context.Cartera
                .Where(c => c.VehiculoId == request.VehiculoId
                            && !c.IsPagado 
                            && request.Vigencias.Contains(c.Vigencia)) // 🚀 Cambiado a Contains
                .ToListAsync();

            if (!carterasAfectadas.Any())
            {
                _logger.LogWarning("No se encontraron deudas para las vigencias seleccionadas.");
                return false;
            }

            // 2. 🚀 GENERACIÓN AUTOMÁTICA DEL CONSECUTIVO DEL AÑO ACTUAL (Optimizado en BD)
            int anioActual = DateTime.UtcNow.Year;

            // Consultamos únicamente el valor máximo actual directamente en la base de datos
            var maxNumeroStr = await _context.Resolucion
                .Where(r => r.FechaCreacion.Year == anioActual)
                .Select(r => r.NumeroResolucion)
                .MaxAsync();

            int siguienteNumero = 1;
            if (!string.IsNullOrEmpty(maxNumeroStr) && int.TryParse(maxNumeroStr, out int maxNumero))
            {
                siguienteNumero = maxNumero + 1;
            }
            
            string numeroResolucionFormateado = siguienteNumero.ToString("D3");
            decimal valorTotalAfectado = carterasAfectadas.Sum(c => c.ValorTotal);

            var nuevaResolucion = new Resolucion
            {
                NumeroResolucion = numeroResolucionFormateado,
                Fecha = DateTime.UtcNow,
                FechaProceso = DateTime.UtcNow,
                TipoResolucion = request.Tipo,
                Valor = valorTotalAfectado,
                Estado = EstadoResolucion.Activa,
                Observaciones = request.Observaciones,
                VehiculoId = request.VehiculoId,
                UsuarioId = 1,

                UsuarioCreo = request.UsuarioId,
                FechaCreacion = DateTime.UtcNow
            };

            // 4. Vincular las carteras y actualizar su trazabilidad
            foreach (var cartera in carterasAfectadas)
            {
                cartera.UsuarioModifico = request.UsuarioId;
                cartera.FechaModificacion = DateTime.UtcNow;
                
                // Si en tu entidad Cartera mantienes la FK explícita 'ResolucionId', se puede asignar aquí.
                // EF se encarga de enlazarlo correctamente al agregar la resolución.
                nuevaResolucion.Carteras.Add(cartera); 
            }

            _context.Resolucion.Add(nuevaResolucion);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            _logger.LogInformation("Resolución Oficial N° {Numero} generada automáticamente.", numeroResolucionFormateado);

            return true;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error al autogenerar y crear la resolución.");
            return false;
        }
    }
}