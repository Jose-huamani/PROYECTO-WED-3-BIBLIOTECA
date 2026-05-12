using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Constants;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.Infrastructure.Persistence;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Proyecto3Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportesController : ControllerBase
{
    private readonly AppDbContext _db;

    public ReportesController(AppDbContext db)
    {
        _db = db;
        // Configurar la licencia comunitaria gratuita de QuestPDF
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpGet("libros/excel")]
    public async Task<IActionResult> ExportarLibrosExcel(CancellationToken ct)
    {
        var libros = await _db.Libros
            .Include(l => l.Autor)
            .Include(l => l.Categoria)
            .AsNoTracking()
            .ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Catálogo de Libros");

        // Cabeceras
        ws.Cell(1, 1).Value = "ID";
        ws.Cell(1, 2).Value = "Título";
        ws.Cell(1, 3).Value = "ISBN";
        ws.Cell(1, 4).Value = "Autor";
        ws.Cell(1, 5).Value = "Categoría";
        ws.Cell(1, 6).Value = "Stock Total";
        ws.Cell(1, 7).Value = "Disponibles";
        ws.Cell(1, 8).Value = "Precio (Bs.)";
        ws.Cell(1, 9).Value = "Código QR";
        ws.Cell(1, 10).Value = "Estado";

        var headerRow = ws.Range(1, 1, 1, 10);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.Navy;
        headerRow.Style.Font.FontColor = XLColor.White;

        // Datos
        for (int i = 0; i < libros.Count; i++)
        {
            var l = libros[i];
            int row = i + 2;
            ws.Cell(row, 1).Value = l.Id;
            ws.Cell(row, 2).Value = l.Titulo;
            ws.Cell(row, 3).Value = l.Isbn;
            ws.Cell(row, 4).Value = l.Autor?.Nombre ?? "";
            ws.Cell(row, 5).Value = l.Categoria?.Nombre ?? "";
            ws.Cell(row, 6).Value = l.CantidadTotal;
            ws.Cell(row, 7).Value = l.CantidadDisponible;
            ws.Cell(row, 8).Value = l.Precio;
            ws.Cell(row, 9).Value = l.CodigoQR;
            ws.Cell(row, 10).Value = l.EstaDisponible ? "Disponible" : "Sin Stock";
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Libros_{DateTime.Now:yyyyMMdd}.xlsx");
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpGet("libros/pdf")]
    public async Task<IActionResult> ExportarLibrosPdf(CancellationToken ct)
    {
        var libros = await _db.Libros
            .Include(l => l.Autor)
            .Include(l => l.Categoria)
            .AsNoTracking()
            .ToListAsync(ct);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(ComposeHeader);
                page.Content().Element(x => ComposeContent(x, libros));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Página ");
                    x.CurrentPageNumber();
                    x.Span(" de ");
                    x.TotalPages();
                });
            });
        });

        var pdfBytes = document.GeneratePdf();
        return File(pdfBytes, "application/pdf", $"ReporteLibros_{DateTime.Now:yyyyMMdd}.pdf");
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(column =>
            {
                column.Item().Text("BIBLIOTECA INTELIGENTE UNIVERSITARIA").FontSize(20).SemiBold().FontColor(Colors.Blue.Darken2);
                column.Item().Text($"Reporte General de Libros - {DateTime.Now:dd/MM/yyyy}").FontSize(14).FontColor(Colors.Grey.Darken2);
            });
        });
    }

    private void ComposeContent(IContainer container, List<Libro> libros)
    {
        container.PaddingVertical(1, Unit.Centimetre).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(30);
                columns.RelativeColumn(3);
                columns.RelativeColumn(2);
                columns.RelativeColumn(2);
                columns.ConstantColumn(50);
                columns.ConstantColumn(50);
                columns.ConstantColumn(50);
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("ID");
                header.Cell().Element(CellStyle).Text("Título");
                header.Cell().Element(CellStyle).Text("Autor");
                header.Cell().Element(CellStyle).Text("Categoría");
                header.Cell().Element(CellStyle).AlignRight().Text("Total");
                header.Cell().Element(CellStyle).AlignRight().Text("Disp.");
                header.Cell().Element(CellStyle).AlignRight().Text("Precio");

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            foreach (var l in libros)
            {
                table.Cell().Element(CellStyle).Text(l.Id.ToString());
                table.Cell().Element(CellStyle).Text(l.Titulo);
                table.Cell().Element(CellStyle).Text(l.Autor?.Nombre ?? "");
                table.Cell().Element(CellStyle).Text(l.Categoria?.Nombre ?? "");
                table.Cell().Element(CellStyle).AlignRight().Text(l.CantidadTotal.ToString());
                table.Cell().Element(CellStyle).AlignRight().Text(l.CantidadDisponible.ToString());
                table.Cell().Element(CellStyle).AlignRight().Text($"{l.Precio:0.00} Bs");

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            }
        });
    }

    [Authorize(Roles = AppRoles.AdminOBibliotecario)]
    [HttpPost("libros/import")]
    public async Task<IActionResult> ImportarLibrosExcel(IFormFile archivo, CancellationToken ct)
    {
        if (archivo == null || archivo.Length == 0)
        {
            return BadRequest(new { mensaje = "El archivo está vacío o no es válido." });
        }

        try
        {
            using var stream = archivo.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheet(1); // Leer la primera hoja
            var rows = ws.RowsUsed().Skip(1); // Omitir cabecera

            int creados = 0;
            int omitidos = 0;

            foreach (var row in rows)
            {
                var titulo = row.Cell(1).GetString()?.Trim();
                var isbn = row.Cell(2).GetString()?.Trim() ?? "";
                var autorNombre = row.Cell(3).GetString()?.Trim();
                var categoriaNombre = row.Cell(4).GetString()?.Trim();
                var total = row.Cell(5).GetValue<int>();
                var precio = row.Cell(6).GetValue<decimal>();

                if (string.IsNullOrWhiteSpace(titulo) || string.IsNullOrWhiteSpace(autorNombre) || string.IsNullOrWhiteSpace(categoriaNombre))
                {
                    omitidos++;
                    continue; // Título, Autor y Categoría son obligatorios para importar
                }

                // Generar QR automático
                var qr = "LIB-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();

                // Buscar o crear Autor
                var autor = await _db.Autores.FirstOrDefaultAsync(a => a.Nombre == autorNombre, ct);
                if (autor == null)
                {
                    autor = new Autor { Nombre = autorNombre };
                    _db.Autores.Add(autor);
                    await _db.SaveChangesAsync(ct);
                }

                // Buscar o crear Categoria
                var categoria = await _db.Categorias.FirstOrDefaultAsync(c => c.Nombre == categoriaNombre, ct);
                if (categoria == null)
                {
                    categoria = new Categoria { Nombre = categoriaNombre };
                    _db.Categorias.Add(categoria);
                    await _db.SaveChangesAsync(ct);
                }

                var libro = new Libro
                {
                    Titulo = titulo,
                    Isbn = isbn,
                    AutorId = autor.Id,
                    CategoriaId = categoria.Id,
                    CantidadTotal = total,
                    CantidadDisponible = total,
                    Precio = precio,
                    CodigoQR = qr,
                    CodigoBarras = "BC" + DateTime.Now.ToString("yyyyMMddHHmmss") + creados
                };

                _db.Libros.Add(libro);
                creados++;
            }

            await _db.SaveChangesAsync(ct);

            return Ok(new { mensaje = $"Importación exitosa. {creados} libros agregados. {omitidos} omitidos por datos incompletos." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { mensaje = "Error al procesar el archivo Excel: " + ex.Message });
        }
    }
}
