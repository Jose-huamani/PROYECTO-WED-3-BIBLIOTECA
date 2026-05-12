using System.ComponentModel.DataAnnotations;

namespace Proyecto3Api.DTOs;

public sealed class LibroCreateRequest
{
    [Required(ErrorMessage = "El titulo es obligatorio")]
    [MaxLength(300)]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ISBN es obligatorio")]
    [MaxLength(32)]
    public string Isbn { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un autor valido")]
    public int AutorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecciona una categoria valida")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "El codigo QR es obligatorio")]
    [MaxLength(200)]
    public string CodigoQR { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? CodigoBarras { get; set; }

    [Range(0, 100000)]
    public int CantidadTotal { get; set; }

    [Range(0, 100000)]
    public int CantidadDisponible { get; set; }

    [Range(0, 1000000)]
    public decimal Precio { get; set; }

    [MaxLength(2048)]
    public string? ImagenUrl { get; set; }

    [MaxLength(2000)]
    public string? Introduccion { get; set; }

    [MaxLength(1000)]
    public string? Descripcion { get; set; }

    [MaxLength(200)]
    public string? Editorial { get; set; }

    public int NumeroPaginas { get; set; }

    [MaxLength(100)]
    public string? Idioma { get; set; }

    [MaxLength(100)]
    public string? EstadoLibro { get; set; }
}

public class ReservaResponse
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int LibroId { get; set; }
    public DateTime FechaReserva { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public int Estado { get; set; }
    public string EstadoNombre { get; set; } = string.Empty;
    public LibroMiniResponse Libro { get; set; } = new();
}

public sealed class ReservaAdminResponse : ReservaResponse
{
    public UsuarioMiniResponse Usuario { get; set; } = new();
    public string? Observaciones { get; set; }
}

public sealed class LibroMiniResponse
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Autor { get; set; }
}

public sealed class UsuarioMiniResponse
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public sealed class VentaDto
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaVenta { get; set; }
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
}

public sealed class VentaCreateRequest
{
    [Required]
    public string MetodoPago { get; set; } = string.Empty;
    public string? ReferenciaPago { get; set; }
}

public sealed class CarritoItemDto
{
    public int Id { get; set; }
    public int LibroId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal => PrecioUnitario * Cantidad;
}

public sealed class CarritoDto
{
    public int Id { get; set; }
    public List<CarritoItemDto> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Subtotal);
}
