using SedsImports.Web.Helpers;
using SedsImports.Web.Models;
using SedsImports.Web.Repositories;

namespace SedsImports.Web.Data
{
    /// <summary>
    /// Datos de ejemplo para el prototipo (Fase 2). Se cargan al iniciar la aplicación.
    /// Las fechas se calculan a partir del día de hoy para que el panel siempre se vea actualizado.
    /// En la Fase 3 estos datos pasan a ser el "seed" de la base de datos.
    /// </summary>
    public static class DatosIniciales
    {
        public const string ContrasenaDemo = "Seds2026";

        public static void Cargar(IServiceProvider servicios)
        {
            var semilla = new Semilla(servicios);
            semilla.Ejecutar();
        }

        private sealed class Semilla
        {
            private readonly IRolRepository _roles;
            private readonly IUsuarioRepository _usuarios;
            private readonly IVehiculoRepository _vehiculos;
            private readonly IRevisionRepository _revisiones;
            private readonly IDocumentacionRepository _documentos;
            private readonly IClienteRepository _clientes;
            private readonly IApartadoRepository _apartados;
            private readonly IVentaRepository _ventas;
            private readonly IBitacoraRepository _bitacora;

            private readonly DateTime _hoy = DateTime.Today;
            private Usuario _mario = null!, _jose = null!, _dania = null!, _ernesto = null!;

            public Semilla(IServiceProvider sp)
            {
                _roles = sp.GetRequiredService<IRolRepository>();
                _usuarios = sp.GetRequiredService<IUsuarioRepository>();
                _vehiculos = sp.GetRequiredService<IVehiculoRepository>();
                _revisiones = sp.GetRequiredService<IRevisionRepository>();
                _documentos = sp.GetRequiredService<IDocumentacionRepository>();
                _clientes = sp.GetRequiredService<IClienteRepository>();
                _apartados = sp.GetRequiredService<IApartadoRepository>();
                _ventas = sp.GetRequiredService<IVentaRepository>();
                _bitacora = sp.GetRequiredService<IBitacoraRepository>();
            }

            public void Ejecutar()
            {
                if (_usuarios.ObtenerTodos().Count > 0) return;

                CargarRolesYUsuarios();
                CargarVendidosHistoricos();
                CargarVehiculosActivos();
            }

            // ───────────────────────── Roles y usuarios ─────────────────────────

            private void CargarRolesYUsuarios()
            {
                var patio = _roles.Agregar(new Rol { Tipo = TipoRol.EncargadoPatio, Nombre = "Encargado de patio", Descripcion = "Registra los vehículos que ingresan al patio." });
                var mecanico = _roles.Agregar(new Rol { Tipo = TipoRol.Mecanico, Nombre = "Mecánico", Descripcion = "Revisa y prepara los vehículos; registra notas y fotografías." });
                var asistente = _roles.Agregar(new Rol { Tipo = TipoRol.AsistenteAdministrativa, Nombre = "Asistente administrativa", Descripcion = "Lleva el control de la documentación y consulta disponibilidad." });
                var dueno = _roles.Agregar(new Rol { Tipo = TipoRol.DuenoVendedor, Nombre = "Dueño / vendedor", Descripcion = "Autoriza disponibilidad, registra apartados y ventas, y administra el sistema." });

                _mario = CrearUsuario("Mario Sánchez", "mario.sanchez@sedimports.com", patio);
                _jose = CrearUsuario("José Hernández", "jose.hernandez@sedimports.com", mecanico);
                _dania = CrearUsuario("Dania Martínez", "dania.martinez@sedimports.com", asistente);
                _ernesto = CrearUsuario("Ernesto Rivas", "ernesto.rivas@sedimports.com", dueno);
            }

            private Usuario CrearUsuario(string nombre, string correo, Rol rol)
            {
                var usuario = _usuarios.Agregar(new Usuario
                {
                    NombreCompleto = nombre,
                    Correo = correo,
                    ContrasenaHash = Seguridad.Hash(ContrasenaDemo),
                    RolId = rol.Id,
                    Rol = rol,
                    Activo = true
                });
                rol.Usuarios.Add(usuario);
                return usuario;
            }

            // ───────────────────────── Vehículos activos (los del prototipo) ─────────────────────────

            private void CargarVehiculosActivos()
            {
                // 1. Recién llegados
                var kia = Registrar("Kia", "Rio", 2012, "Blanco", "KNADH4A37C6301582", Hora(0, 11, 8), "Puerta de pasajero delantera con daño.", 3);
                Documentar(kia, Hora(0, 11, 20), false, false, false, false, EstadoDocumentacion.Pendiente, "Sin documentos registrados.");

                var hilux = Registrar("Toyota", "Hilux", 2019, "Blanco", "MR0FB3CD5K0123456", Hora(0, 8, 30), "Golpe leve en defensa trasera. Llegó con 112,000 km.", 3);
                Documentar(hilux, Hora(0, 8, 52), false, true, false, false, EstadoDocumentacion.Pendiente, "Pendiente documento original de propiedad.");

                var vitara = Registrar("Suzuki", "Vitara", 2021, "Plata", "JS3TD62S6M4012345", Hora(-1, 16, 40), "Rayón en puerta trasera derecha. 45,800 km.", 2);
                Documentar(vitara, Hora(-1, 17, 0), false, false, false, false, EstadoDocumentacion.Pendiente, "Aún no se reciben documentos.");

                // 2. En revisión / preparación
                var crv = Registrar("Honda", "CR-V", 2018, "Gris", "2HKRW2H59JH654321", Hora(-2, 15, 10), "Testigo de motor encendido al llegar. 98,500 km.", 3);
                Documentar(crv, Hora(-2, 15, 40), true, true, true, false, EstadoDocumentacion.Pendiente, "Pendiente pago de trámite de traspaso.");
                IniciarRevision(crv, Hora(-1, 9, 20));
                Nota(crv, Hora(-1, 9, 45), "Se reemplazaron las bujías porque las anteriores estaban deterioradas.");
                Nota(crv, Hora(0, 10, 40), "Cambio de aceite y filtros realizado.");
                Fotos(crv, TipoFoto.Revision, Hora(-1, 9, 50), "Bujías reemplazadas", "Motor limpio");

                var elantra = Registrar("Hyundai", "Elantra", 2019, "Azul", "KMHD84LF5KU678901", Hora(-3, 9, 30), "Llantas delanteras desgastadas. 76,200 km.", 3);
                Documentar(elantra, Hora(-3, 10, 0), true, true, true, true, EstadoDocumentacion.Completa, "Expediente completo.");
                IniciarRevision(elantra, Hora(-2, 8, 15));
                Nota(elantra, Hora(-2, 11, 30), "Alineación y balanceo realizados.");
                Nota(elantra, Hora(-1, 14, 5), "Pastillas de freno delanteras nuevas.");

                // 3. Revisión terminada: espera autorización del dueño (nueva regla del negocio)
                var equinox = Registrar("Chevrolet", "Equinox", 2019, "Negro", "2GNAXKEV1K6123987", Hora(-8, 10, 15), "Faros opacos y escobillas dañadas. 88,900 km.", 3);
                Documentar(equinox, Hora(-7, 9, 0), true, true, true, true, EstadoDocumentacion.Completa, "Expediente completo.");
                IniciarRevision(equinox, Hora(-6, 8, 30));
                Nota(equinox, Hora(-6, 12, 0), "Cambio de aceite, filtro de aire y escobillas.");
                Nota(equinox, Hora(-3, 15, 20), "Pulido de faros delanteros terminado.");
                Finalizar(equinox, Hora(-1, 17, 10),
                    "Mantenimiento general completo: aceite, filtros, escobillas y pulido de faros.",
                    "Falta cambiar la llanta de repuesto y revisar el seguro de la puerta trasera izquierda.", 3);

                // 4. Disponibles para venta
                var cx5 = Registrar("Mazda", "CX-5", 2020, "Rojo", "JM3KFBCM5L0789012", Hora(-26, 11, 20), "Pintura en buen estado. 64,300 km.", 3);
                Documentar(cx5, Hora(-22, 9, 5), true, true, true, true, EstadoDocumentacion.Completa, "Expediente completo.");
                IniciarRevision(cx5, Hora(-25, 8, 0));
                Nota(cx5, Hora(-24, 10, 0), "Revisión general sin fallas mayores.");
                Nota(cx5, Hora(-23, 15, 0), "Pulido y limpieza interior terminados.");
                Finalizar(cx5, Hora(-23, 16, 30), "Revisión general y limpieza completa.", "Ninguno.", 3);
                Autorizar(cx5, Hora(-22, 8, 30));

                var sportage = Registrar("Kia", "Sportage", 2019, "Azul", "KNDPMCAC5K7234567", Hora(-34, 10, 0), "Sin golpes visibles. 70,100 km.", 3);
                Documentar(sportage, Hora(-29, 10, 10), true, true, true, true, EstadoDocumentacion.Completa, "Expediente completo.");
                IniciarRevision(sportage, Hora(-33, 8, 0));
                Nota(sportage, Hora(-32, 11, 0), "Cambio de amortiguadores traseros.");
                Finalizar(sportage, Hora(-31, 16, 0), "Suspensión trasera reparada y revisión general.", "Ninguno.", 3);
                Autorizar(sportage, Hora(-30, 9, 0));

                var frontier = Registrar("Nissan", "Frontier", 2017, "Negro", "1N6AD0EV5HN345678", Hora(-41, 9, 45), "Batea con rayones leves. 120,400 km.", 3);
                Documentar(frontier, Hora(-38, 10, 20), true, true, true, true, EstadoDocumentacion.Completa, "Expediente completo.");
                IniciarRevision(frontier, Hora(-40, 8, 0));
                Nota(frontier, Hora(-39, 14, 0), "Cambio de banda de distribución.");
                Finalizar(frontier, Hora(-39, 17, 0), "Banda de distribución nueva y revisión de frenos.", "Ninguno.", 2);
                Autorizar(frontier, Hora(-38, 8, 45));

                // 5. Apartados
                var tucson = Registrar("Hyundai", "Tucson", 2021, "Blanco", "5NMJB3AE4MH456789", Hora(-53, 10, 30), "Excelente estado. 38,000 km.", 3);
                Documentar(tucson, Hora(-50, 9, 0), true, true, true, true, EstadoDocumentacion.Completa, "Expediente completo.");
                IniciarRevision(tucson, Hora(-52, 8, 0));
                Nota(tucson, Hora(-51, 10, 0), "Detallado interior y exterior.");
                Finalizar(tucson, Hora(-51, 16, 0), "Detallado completo; sin fallas mecánicas.", "Ninguno.", 3);
                Autorizar(tucson, Hora(-50, 8, 30));
                Apartar(tucson, Hora(-22, 11, 0), "Carlos Menjívar", "7777-0022", 1500, "Pagará el resto con financiamiento.");

                var rav4 = Registrar("Toyota", "RAV4", 2018, "Plata", "2T3BFREV4JW567890", Hora(-57, 9, 15), "Parabrisas con estrella pequeña. 91,000 km.", 3);
                IniciarRevision(rav4, Hora(-56, 8, 0));
                Nota(rav4, Hora(-55, 9, 30), "Reparación de parabrisas con resina.");
                Finalizar(rav4, Hora(-54, 16, 0), "Parabrisas reparado y revisión general.", "Ninguno.", 3);
                Autorizar(rav4, Hora(-53, 8, 0));
                Apartar(rav4, Hora(-23, 11, 40), "Ana Lissette Coto", "7122-9034", 2000, "Retira el vehículo a fin de mes.");
                Documentar(rav4, Hora(-1, 10, 15), true, true, true, true, EstadoDocumentacion.Completa, "Expediente completo.");
            }

            // ───────────────────────── Ventas anteriores (para el reporte) ─────────────────────────

            private void CargarVendidosHistoricos()
            {
                var historicos = new (int MesesAtras, int Dia, string Marca, string Modelo, int Anio, string Color, string Vin, string Cliente, string Tel, decimal Monto)[]
                {
                    (8, 14, "Honda", "Accord", 2016, "Gris", "1HGCR2F33GA112233", "Ricardo Pineda", "7012-3344", 13200),
                    (7, 9, "Kia", "Forte", 2019, "Rojo", "3KPF24AD7KE223344", "Gabriela López", "7155-6677", 11500),
                    (6, 6, "Mazda", "3", 2018, "Azul", "3MZBN1U73JM334455", "Karla Mejía", "7288-9900", 12300),
                    (6, 20, "Toyota", "Tacoma", 2017, "Blanco", "5TFAZ5CN1HX445566", "Fernando Cruz", "7311-2233", 24000),
                    (5, 17, "Nissan", "Versa", 2020, "Plata", "3N1CN8EV2LL556677", "Diego Portillo", "7444-5566", 10900),
                    (4, 8, "Hyundai", "Accent", 2018, "Blanco", "KMHCT4AE2JU667788", "Pedro Molina", "7577-8899", 9400),
                    (4, 25, "Chevrolet", "Trax", 2019, "Negro", "KL7CJPSB1KB778899", "Andrea Flores", "7600-1122", 13800),
                    (3, 12, "Toyota", "Yaris", 2019, "Rojo", "MHKA4DE29KJ889900", "Sofía Rivas", "7733-4455", 11200),
                    (2, 10, "Honda", "Fit", 2016, "Azul", "JHMGK5H51GX990011", "Jorge Ayala", "7866-7788", 8900),
                    (2, 28, "Ford", "Ranger", 2020, "Gris", "MNCUMFF80LW890123", "Luis Hernández", "7999-0011", 27500),
                    (1, 5, "Mitsubishi", "L200", 2019, "Blanco", "MMBJNKB40KD001122", "Roberto Alvarado", "7020-3040", 21000),
                    (1, 22, "Kia", "Soul", 2018, "Verde", "KNDJN2A27J7112233", "Marta Guzmán", "7050-6070", 9800),
                    (0, -3, "Toyota", "Corolla", 2021, "Blanco", "JTDBR32E220123456", "Carlos Hernández", "7080-9010", 15200),
                    (0, -1, "Honda", "Civic", 2017, "Negro", "2HGFC2F59HH223344", "Laura Pérez", "7101-1213", 16700)
                };

                foreach (var h in historicos)
                {
                    var fechaVenta = FechaVenta(h.MesesAtras, h.Dia);
                    var ingreso = fechaVenta.AddDays(-30).AddHours(9);

                    var v = Registrar(h.Marca, h.Modelo, h.Anio, h.Color, h.Vin, ingreso, "Ingreso registrado en el patio.", 3);
                    Documentar(v, ingreso.AddDays(2).AddHours(1), true, true, true, true, EstadoDocumentacion.Completa, "Expediente completo.");
                    IniciarRevision(v, ingreso.AddDays(1));
                    Nota(v, ingreso.AddDays(3).AddHours(2), "Revisión general y mantenimiento preventivo.");
                    Finalizar(v, ingreso.AddDays(6).AddHours(7), "Mantenimiento preventivo completo.", "Ninguno.", 3);
                    Autorizar(v, ingreso.AddDays(7));
                    var apartado = Apartar(v, fechaVenta.AddDays(-7).AddHours(11), h.Cliente, h.Tel, Math.Round(h.Monto * 0.1m, 0), null);
                    Vender(v, fechaVenta.AddHours(12).AddMinutes(40), apartado, h.Monto);
                }
            }

            /// <summary>Fecha de una venta: N meses atrás. Si el día es negativo, cuenta días hacia atrás desde hoy dentro del mes actual.</summary>
            private DateTime FechaVenta(int mesesAtras, int dia)
            {
                if (mesesAtras == 0)
                {
                    var fecha = _hoy.AddDays(dia);
                    return fecha.Month == _hoy.Month ? fecha : new DateTime(_hoy.Year, _hoy.Month, 1);
                }

                var mes = _hoy.AddMonths(-mesesAtras);
                var diaValido = Math.Min(dia, DateTime.DaysInMonth(mes.Year, mes.Month));
                return new DateTime(mes.Year, mes.Month, diaValido);
            }

            // ───────────────────────── Pasos del proceso (cada uno queda en la bitácora) ─────────────────────────

            private DateTime Hora(int dias, int hora, int minuto)
            {
                var fecha = _hoy.AddDays(dias).AddHours(hora).AddMinutes(minuto);
                // Si la aplicación se abre temprano, evita que un registro de "hoy" quede en el futuro.
                var limite = DateTime.Now.AddMinutes(-5);
                return fecha > limite ? limite : fecha;
            }

            private Vehiculo Registrar(string marca, string modelo, int anio, string color, string vin, DateTime fecha, string detalles, int fotosIngreso)
            {
                var v = _vehiculos.Agregar(new Vehiculo
                {
                    Marca = marca, Modelo = modelo, Anio = anio, Color = color, Vin = vin,
                    Detalles = detalles, FechaIngreso = fecha, Estado = EstadoVehiculo.RecienLlegado,
                    RegistradoPorId = _mario.Id, RegistradoPorNombre = _mario.NombreCompleto
                });
                Fotos(v, TipoFoto.Ingreso, fecha, new[] { "Frente", "Lateral", "Interior" }.Take(fotosIngreso).ToArray());
                _documentos.Agregar(new Documentacion { VehiculoId = v.Id, Estado = EstadoDocumentacion.Pendiente, Observaciones = "Sin documentos registrados." });
                Log(_mario, v, fecha, TipoMovimiento.Registro, "Registró vehículo · Estado inicial: Recién llegado");
                return v;
            }

            private void Fotos(Vehiculo v, TipoFoto tipo, DateTime fecha, params string[] descripciones)
            {
                var autor = tipo == TipoFoto.Ingreso ? _mario.NombreCompleto : _jose.NombreCompleto;
                foreach (var d in descripciones)
                {
                    v.Fotografias.Add(new Fotografia { Id = _vehiculos.SiguienteIdFoto(), Tipo = tipo, Descripcion = d, Fecha = fecha, SubidaPor = autor });
                }
            }

            private void Documentar(Vehiculo v, DateTime fecha, bool titulo, bool importacion, bool tarjeta, bool traspaso, EstadoDocumentacion estado, string obs)
            {
                var doc = _documentos.ObtenerPorVehiculo(v.Id)!;
                doc.TieneTitulo = titulo;
                doc.TieneImportacion = importacion;
                doc.TieneTarjetaCirculacion = tarjeta;
                doc.TieneTraspaso = traspaso;
                doc.Estado = estado;
                doc.Observaciones = obs;
                doc.FechaActualizacion = fecha;
                doc.ActualizadoPor = _dania.NombreCompleto;
                Log(_dania, v, fecha, TipoMovimiento.Documentacion, $"Actualizó documentación a {estado.NombreVisible()}");
            }

            private void IniciarRevision(Vehiculo v, DateTime fecha)
            {
                _revisiones.Agregar(new Revision
                {
                    VehiculoId = v.Id, MecanicoId = _jose.Id, MecanicoNombre = _jose.NombreCompleto,
                    FechaInicio = fecha, FechaActualizacion = fecha
                });
                v.Estado = EstadoVehiculo.EnRevision;
                Log(_jose, v, fecha, TipoMovimiento.Revision, "Inició revisión · Recién llegado → En revisión/preparación");
            }

            private void Nota(Vehiculo v, DateTime fecha, string texto)
            {
                var r = _revisiones.ObtenerPorVehiculo(v.Id)!;
                r.Notas.Add(new NotaRevision { Id = _revisiones.SiguienteIdNota(), UsuarioId = _jose.Id, UsuarioNombre = _jose.NombreCompleto, Fecha = fecha, Texto = texto });
                r.FechaActualizacion = fecha;
                Log(_jose, v, fecha, TipoMovimiento.Revision, $"Agregó anotación: {texto}");
            }

            private void Finalizar(Vehiculo v, DateTime fecha, string resumen, string pendientes, int fotosFinales)
            {
                var r = _revisiones.ObtenerPorVehiculo(v.Id)!;
                r.ResumenFinal = resumen;
                r.Pendientes = pendientes;
                r.FechaFinalizacion = fecha;
                r.FechaActualizacion = fecha;
                v.Estado = EstadoVehiculo.RevisionFinalizada;
                Fotos(v, TipoFoto.Final, fecha, new[] { "Frente", "Lateral", "Interior" }.Take(fotosFinales).ToArray());
                Log(_jose, v, fecha, TipoMovimiento.Revision, "Finalizó revisión · En revisión → Pendiente de autorización");
            }

            private void Autorizar(Vehiculo v, DateTime fecha)
            {
                var r = _revisiones.ObtenerPorVehiculo(v.Id)!;
                r.FechaAutorizacion = fecha;
                r.AutorizadoPor = _ernesto.NombreCompleto;
                v.Estado = EstadoVehiculo.Disponible;
                Log(_ernesto, v, fecha, TipoMovimiento.Autorizacion, "Autorizó disponibilidad · Pendiente de autorización → Disponible para venta");
            }

            private Apartado Apartar(Vehiculo v, DateTime fecha, string cliente, string telefono, decimal anticipo, string? obs)
            {
                var c = _clientes.Agregar(new Cliente { Nombre = cliente, Telefono = telefono });
                var a = _apartados.Agregar(new Apartado
                {
                    VehiculoId = v.Id, ClienteId = c.Id, FechaApartado = fecha, Anticipo = anticipo,
                    Observaciones = obs, RegistradoPor = _ernesto.NombreCompleto
                });
                v.Estado = EstadoVehiculo.Apartado;
                Log(_ernesto, v, fecha, TipoMovimiento.Comercial, $"Registró apartado · Cliente: {cliente} · Anticipo: {Formato.Dinero(anticipo)}");
                return a;
            }

            private void Vender(Vehiculo v, DateTime fecha, Apartado apartado, decimal monto)
            {
                apartado.Estado = EstadoApartado.ConvertidoEnVenta;
                apartado.FechaCierre = fecha;
                var cliente = _clientes.ObtenerPorId(apartado.ClienteId)!;
                _ventas.Agregar(new Venta
                {
                    VehiculoId = v.Id, ClienteId = cliente.Id, ApartadoId = apartado.Id, FechaVenta = fecha.Date,
                    Monto = monto, RegistradoPor = _ernesto.NombreCompleto, FechaRegistro = fecha
                });
                v.Estado = EstadoVehiculo.Vendido;
                Log(_ernesto, v, fecha, TipoMovimiento.Comercial, $"Registró venta · Cliente: {cliente.Nombre} · Monto: {Formato.Dinero(monto)}");
            }

            private void Log(Usuario u, Vehiculo v, DateTime fecha, TipoMovimiento tipo, string accion)
            {
                _bitacora.Agregar(new MovimientoBitacora
                {
                    Fecha = fecha, UsuarioId = u.Id, UsuarioNombre = u.NombreCompleto,
                    RolNombre = u.Rol.Nombre, RolTipo = u.Rol.Tipo,
                    VehiculoId = v.Id, VehiculoNombre = v.NombreCompleto, Tipo = tipo, Accion = accion
                });
            }
        }
    }
}
