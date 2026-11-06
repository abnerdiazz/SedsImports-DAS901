using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SedsImports.Infrastructure.Datos.Migraciones
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Correo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreCompleto = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    ContrasenaHash = table.Column<string>(type: "nchar(64)", fixedLength: true, maxLength: 64, nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Vehiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Vin = table.Column<string>(type: "nvarchar(17)", maxLength: 17, nullable: false),
                    Marca = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Modelo = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Anio = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Detalles = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RegistradoPorId = table.Column<int>(type: "int", nullable: false),
                    RegistradoPorNombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FechaUltimaEdicion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EditadoPorNombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Eliminado = table.Column<bool>(type: "bit", nullable: false),
                    FechaEliminacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EliminadoPorNombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehiculos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vehiculos_Usuarios_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Apartados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehiculoId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    FechaApartado = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Anticipo = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    RegistradoPor = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FechaCierre = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MotivoCancelacion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apartados", x => x.Id);
                    table.CheckConstraint("CK_Apartados_Anticipo", "[Anticipo] > 0");
                    table.ForeignKey(
                        name: "FK_Apartados_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Apartados_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Documentaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehiculoId = table.Column<int>(type: "int", nullable: false),
                    TieneTitulo = table.Column<bool>(type: "bit", nullable: false),
                    TieneImportacion = table.Column<bool>(type: "bit", nullable: false),
                    TieneTarjetaCirculacion = table.Column<bool>(type: "bit", nullable: false),
                    TieneTraspaso = table.Column<bool>(type: "bit", nullable: false),
                    TieneOtros = table.Column<bool>(type: "bit", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documentaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documentaciones_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Fotografias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehiculoId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Ruta = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    Descripcion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubidaPor = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fotografias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fotografias_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosBitacora",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    UsuarioNombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    RolNombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    RolTipo = table.Column<int>(type: "int", nullable: true),
                    VehiculoId = table.Column<int>(type: "int", nullable: true),
                    VehiculoNombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosBitacora", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosBitacora_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosBitacora_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Revisiones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehiculoId = table.Column<int>(type: "int", nullable: false),
                    MecanicoId = table.Column<int>(type: "int", nullable: true),
                    MecanicoNombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FechaFinalizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResumenFinal = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    Pendientes = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    FechaAutorizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutorizadoPor = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Revisiones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Revisiones_Usuarios_MecanicoId",
                        column: x => x.MecanicoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Revisiones_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehiculoId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    ApartadoId = table.Column<int>(type: "int", nullable: true),
                    FechaVenta = table.Column<DateTime>(type: "date", nullable: false),
                    Monto = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true),
                    RegistradoPor = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                    table.CheckConstraint("CK_Ventas_Monto", "[Monto] IS NULL OR [Monto] >= 0");
                    table.ForeignKey(
                        name: "FK_Ventas_Apartados_ApartadoId",
                        column: x => x.ApartadoId,
                        principalTable: "Apartados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Ventas_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotasRevision",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RevisionId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    UsuarioNombre = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Texto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EsDelSistema = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotasRevision_Revisiones_RevisionId",
                        column: x => x.RevisionId,
                        principalTable: "Revisiones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotasRevision_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Apartados_ClienteId",
                table: "Apartados",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Apartados_VehiculoId_Estado",
                table: "Apartados",
                columns: new[] { "VehiculoId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Nombre_Telefono",
                table: "Clientes",
                columns: new[] { "Nombre", "Telefono" });

            migrationBuilder.CreateIndex(
                name: "IX_Documentaciones_VehiculoId",
                table: "Documentaciones",
                column: "VehiculoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Fotografias_VehiculoId",
                table: "Fotografias",
                column: "VehiculoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBitacora_Fecha",
                table: "MovimientosBitacora",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBitacora_UsuarioId",
                table: "MovimientosBitacora",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosBitacora_VehiculoId_Fecha",
                table: "MovimientosBitacora",
                columns: new[] { "VehiculoId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_NotasRevision_RevisionId",
                table: "NotasRevision",
                column: "RevisionId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasRevision_UsuarioId_Fecha",
                table: "NotasRevision",
                columns: new[] { "UsuarioId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_Revisiones_MecanicoId",
                table: "Revisiones",
                column: "MecanicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Revisiones_VehiculoId",
                table: "Revisiones",
                column: "VehiculoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Tipo",
                table: "Roles",
                column: "Tipo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Correo",
                table: "Usuarios",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_Estado",
                table: "Vehiculos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_FechaIngreso",
                table: "Vehiculos",
                column: "FechaIngreso");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_RegistradoPorId",
                table: "Vehiculos",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_Vin",
                table: "Vehiculos",
                column: "Vin",
                unique: true,
                filter: "[Eliminado] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ApartadoId",
                table: "Ventas",
                column: "ApartadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_ClienteId",
                table: "Ventas",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_FechaVenta",
                table: "Ventas",
                column: "FechaVenta");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_VehiculoId",
                table: "Ventas",
                column: "VehiculoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Documentaciones");

            migrationBuilder.DropTable(
                name: "Fotografias");

            migrationBuilder.DropTable(
                name: "MovimientosBitacora");

            migrationBuilder.DropTable(
                name: "NotasRevision");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "Revisiones");

            migrationBuilder.DropTable(
                name: "Apartados");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Vehiculos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
