using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Proyecto3Api.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddLibroMissingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Libros",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Editorial",
                table: "Libros",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoLibro",
                table: "Libros",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Idioma",
                table: "Libros",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Introduccion",
                table: "Libros",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroPaginas",
                table: "Libros",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Descripcion",
                table: "Libros");

            migrationBuilder.DropColumn(
                name: "Editorial",
                table: "Libros");

            migrationBuilder.DropColumn(
                name: "EstadoLibro",
                table: "Libros");

            migrationBuilder.DropColumn(
                name: "Idioma",
                table: "Libros");

            migrationBuilder.DropColumn(
                name: "Introduccion",
                table: "Libros");

            migrationBuilder.DropColumn(
                name: "NumeroPaginas",
                table: "Libros");
        }
    }
}
