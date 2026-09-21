using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rodamiento.Controller
{
    [ApiController]
    [Route("api/[controller]")]
    public class ArchivosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string _archivosFolder;

        public ArchivosController(IConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _archivosFolder = _configuration.GetValue<string>("FileStorageTemplate") ?? throw new ArgumentNullException("FileStorageTemplate no está configurado.");
        }

        [HttpGet("{fileName}")]
        public async Task<IActionResult> GetFile(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest("El nombre del archivo no puede estar vacío.");
            }

            try
            {
                var filePath = Path.Combine(_archivosFolder, fileName);

                Console.WriteLine($"Verificando existencia del archivo en: '{filePath}'");

                if (!Directory.Exists(_archivosFolder))
                {
                    Console.WriteLine("⚠️ La carpeta especificada no existe.");
                    return StatusCode(500, "La carpeta de almacenamiento no existe.");
                }

                if (!System.IO.File.Exists(filePath))
                {
                    // Fallback flexible para localizar recibos ya sea que se busquen por:
                    // - Recibo_123.pdf
                    // - Recibo_ABC123_20260920_123.pdf
                    // - o cualquier combinación que contenga el ID
                    if (fileName.StartsWith("Recibo_", StringComparison.OrdinalIgnoreCase))
                    {
                        var rawName = Path.GetFileNameWithoutExtension(fileName);

                        // 1. Búsqueda directa por coincidencia parcial del nombre
                        var matches = Directory.GetFiles(_archivosFolder, $"*{rawName}*.pdf");

                        // 2. Extraer el ID numérico del recibo (ej: 123 en "Recibo_123" o "Recibo_ABC123_20260920_123")
                        if (!matches.Any())
                        {
                            var matchId = Regex.Match(rawName, @"(?:^Recibo_|_)(?<id>\d+)$");
                            if (matchId.Success && int.TryParse(matchId.Groups["id"].Value, out var id))
                            {
                                matches = Directory.GetFiles(_archivosFolder, $"Recibo_*_{id}.pdf")
                                    .Concat(Directory.GetFiles(_archivosFolder, $"Recibo_{id}.pdf"))
                                    .Distinct()
                                    .ToArray();
                            }
                        }

                        if (matches.Any())
                        {
                            filePath = matches.OrderByDescending(f => System.IO.File.GetLastWriteTimeUtc(f)).First();
                            Console.WriteLine($"🔍 Archivo localizado por fallback: '{filePath}'");
                        }
                    }
                }

                if (!System.IO.File.Exists(filePath))
                {
                    Console.WriteLine($"⚠️ El archivo {fileName} no se encontró en el servidor.");
                    return NotFound($"El archivo {fileName} no existe.");
                }

                var memory = new MemoryStream();
                await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    await stream.CopyToAsync(memory);
                }
                memory.Position = 0;

                Console.WriteLine($"✅ Archivo {fileName} leído correctamente.");

                return File(memory, "application/pdf", fileName);
            }
            catch (UnauthorizedAccessException)
            {
                Console.WriteLine("❌ Error: Permiso denegado para acceder al archivo.");
                return StatusCode(403, "No tienes permisos para acceder a este archivo.");
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"❌ Error: El archivo {fileName} no fue encontrado.");
                return NotFound($"El archivo {fileName} no existe.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"❌ Error de I/O al leer el archivo: {ex.Message}");
                return StatusCode(500, "Error de lectura del archivo.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error inesperado: {ex.Message}");
                return StatusCode(500, "Error interno del servidor.");
            }
        }
    }
}
