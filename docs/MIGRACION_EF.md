# Migracion a Entity Framework

## Objetivo

Crear una base nueva normalizada, con llaves foraneas reales, y mover la logica que hoy vive en funciones de PostgreSQL hacia services de C# con nombres de negocio.

## Estado de la base legacy `Rodamientos`

- 27 tablas.
- 29 funciones `plpgsql`.
- No hay foreign keys declaradas.
- No hay vistas.
- Las relaciones son implicitas por columnas como `placa`, `marca`, `clase`, `linea`, `tipo_id`, `estado`, `recibo` y `concepto`.

Tablas con mas peso:

- `cartera`: deuda generada por placa, vigencia y concepto.
- `vehiculos`: maestro de vehiculos.
- `propietarios`: maestro de propietarios.
- `recibos`: recibos pendientes, anulados y pagados.
- `procesos`: persuasivo/coactivo.
- `resoluciones`: novedades/resoluciones.

## Relaciones implicitas que deben volverse FK

- `vehiculos.marca` -> `marcas.id`
- `vehiculos.linea` -> `lineas.id`
- `vehiculos.clase` -> `clases_veh.id`
- `vehiculos.ncolor` -> `colores.id`
- `vehiculos.tipo_ser` -> `tipo_servicios.id`
- `vehiculos.tipo_id` -> `tipos_ident.id`
- `vehiculos.estado` -> `estados.id`
- `vehiculos.documento + tipo_id` -> `propietarios.documento + tipo_id`
- `cartera.placa` -> `vehiculos.placa` o, en el modelo nuevo, `cartera.vehiculo_id` -> `vehiculos.id`
- `cartera.concepto` -> `conceptos.id`
- `recibos.placa` -> `vehiculos.placa` o, en el modelo nuevo, `recibos.vehiculo_id` -> `vehiculos.id`
- `procesos.placa` -> `vehiculos.placa` o, en el modelo nuevo, `procesos.vehiculo_id` -> `vehiculos.id`
- `procesos.estado` -> `estados_proc.id`
- `resoluciones.placa` -> `vehiculos.placa` o, en el modelo nuevo, `resoluciones.vehiculo_id` -> `vehiculos.id`
- `resoluciones.tipo` -> `tipos_nov.id`
- `resoluciones.usuario` -> `usuarios.id`

## Funciones legacy y nombres recomendados en services

- `calcula_int` -> `CalcularInteresMoraAsync`
- `carga_impuesto_placa` -> `GenerarCarteraVehiculoAsync`
- `carga_impuesto` -> `GenerarCarteraMasivaAsync`
- `carga_sancion` -> `GenerarSancionesAsync`
- `muestra_deuda` -> `ObtenerDetalleDeudaAsync`
- `deuda_x_conceptos` -> `LiquidarDeudaPorConceptosAsync`
- `resumen_x_conceptos` -> `ObtenerResumenConceptosAsync`
- `graba_recibo` -> `GenerarReciboAsync`
- `p_bancos` -> `AplicarPagoReciboAsync`
- `reversa_recibo` -> `ReversarPagoReciboAsync`
- `persuasivo` -> `CrearProcesosPersuasivosAsync`
- `mandamiento` -> `EscalarPersuasivoACoactivoAsync`
- `sin_proceso` -> `ObtenerCarteraSinProcesoAsync`
- `consulta_placa` -> `ConsultarEstadoVehiculoAsync`
- `graba_resol` -> `RegistrarResolucionAsync`
- `informe_diario` -> `GenerarReporteRecaudoDiarioAsync`
- `informe_cartera` -> `GenerarReporteCarteraAsync`
- `deuda_x_periodos` -> `ObtenerDeudaPorVigenciaAsync`
- `busca_placas` -> `BuscarVehiculosPorPropietarioAsync`
- `tcrea_propiet` -> `CrearPropietarioSiNoExisteAsync`
- `existe_base` -> `ExisteBaseGravableAsync`

## Reglas de negocio detectadas

- La cartera se genera por vigencia y concepto.
- Concepto `1`: rodamiento/transito.
- Concepto `2`: estampillas, calculadas como 2% de rodamiento + carga.
- Concepto `3`: costas/recibo, tomado de `estados_proc.costas`.
- Concepto `4`: impuesto/cargo adicional por carga o pasajeros para servicio publico.
- Concepto `6`: sancion, calculada desde `parametros.porc_sancion`.
- Los intereses se calculan por mes usando la tabla `intereses`, capitalizando por dias y aplicando el 25% del resultado acumulado.
- El descuento vigente sale de `descuentos` por rango de fechas y se aplica sobre el interes.
- El valor de sistematizacion sale de `parametros.vlr_sistem` y se agrega solo en la ultima vigencia liquidada.
- Al generar un recibo pendiente se anulan recibos pendientes anteriores de la misma placa.
- Al aplicar pago se marca cartera como pagada, recibo como cancelado, proceso como cerrado y se actualiza `vehiculos.pago_hasta`.
- Al reversar pago se vuelve a marcar la cartera como no pagada y el recibo como pendiente.
- Persuasivo crea registros en `procesos` para cartera no pagada, no coactiva, y vehiculo activo.
- Mandamiento cambia procesos persuasivos a coactivos y recalcula costas.

## Riesgos encontrados

- El codigo C# actual no coincide con algunas firmas reales de la BD:
  - `p_bancos` recibe `recibo`, `pplaca`, `xdesde`, `xhasta`.
  - `graba_recibo` recibe `pplaca`, `pcedula`, `ptipo_id`, `pdesde`, `phasta`.
- `graba_resol` intenta actualizar `vehiculos.activo`, pero esa columna no existe en la tabla legacy actual.
- La BD no protege integridad referencial; se debe limpiar/validar data antes de crear FKs.
- Hay SQL dinamico en varias funciones. Al moverlo a services debe convertirse en LINQ parametrizado.

## Orden recomendado

1. Corregir configuracion del `DbContext` para usar `appsettings`.
2. Separar modelo legacy y modelo nuevo si se necesita leer de la BD vieja y escribir en la nueva.
3. Definir entidades normalizadas y relaciones FK.
4. Migrar catalogos: marcas, lineas, colores, tipos, estados, conceptos, tarifas, intereses, descuentos, parametros.
5. Migrar vehiculos y propietarios.
6. Migrar cartera y liquidacion.
7. Migrar recibos, pagos y reversas.
8. Migrar procesos persuasivos/coactivos y resoluciones.
9. Reemplazar llamadas a funciones por services con nombres de negocio.

## Avance implementado

- `MainDataContext` ya no tiene cadena de conexion hardcodeada.
- `Program.cs` usa `DefaultConnection` desde `appsettings`.
- `EstadoRecibo` incluye `Cancelado`, equivalente al estado legacy `C`.
- `Tarifa` incluye `ConceptoTarifa` para reemplazar el significado escondido de `tarifas.clase`.
- `Cartera` incluye `VehiculoId` y `EstaEnProcesoCoactivo` para normalizar la relacion y reemplazar `cartera.coactivo`.
- `LiquidacionService` ya tiene logica C# para:
  - `CalcularInteresMoraAsync`
  - `ObtenerDetalleDeudaAsync`
  - `LiquidarDeudaPorConceptosAsync`
  - `GenerarReciboAsync`
  - `GenerarCarteraVehiculoAsync`
- `PagosService` ya tiene logica C# para:
  - `AplicarPagoReciboAsync`
  - `ReversarPagoReciboAsync`
- `CoactivoService` ya tiene logica C# para:
  - `CrearProcesosPersuasivosAsync`
  - `EscalarPersuasivoAMandamientoAsync`
