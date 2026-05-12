using System.ComponentModel.DataAnnotations;

namespace Proyecto3Api.DTOs;

public sealed class LineaPrestamoDto
{
    [Range(1, int.MaxValue)]
    public int LibroId { get; set; }

    [Range(1, 100)]
    public int Cantidad { get; set; } = 1;
}

public sealed class CrearPrestamoRequest
{
    /// <summary>Solo Administrador/Bibliotecario pueden fijar otro usuario.</summary>
    public int? UsuarioId { get; set; }

    [Range(1, 90)]
    public int DiasPrestamo { get; set; } = 14;

    [Required, MinLength(1)]
    public List<LineaPrestamoDto> Lineas { get; set; } = new();
}
