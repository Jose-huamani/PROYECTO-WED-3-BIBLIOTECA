using System.ComponentModel.DataAnnotations.Schema;

namespace Proyecto3Api.Domain.Entities;

public class Libro
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int AutorId { get; set; }
    public int CategoriaId { get; set; }
    public string CodigoQR { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    /// <summary>Ejemplares en inventario (total).</summary>
    public int CantidadTotal { get; set; }
    /// <summary>Ejemplares disponibles para préstamo.</summary>
    public int CantidadDisponible { get; set; }
    public decimal Precio { get; set; }
    [NotMapped]
    public bool EstaDisponible => CantidadDisponible > 0;
    public string? ImagenUrl { get; set; }
    
    public string? Introduccion { get; set; }
    public string? Descripcion { get; set; }
    public string? Editorial { get; set; }
    public int NumeroPaginas { get; set; }
    public string? Idioma { get; set; }
    public string? EstadoLibro { get; set; }

    public Autor? Autor { get; set; }
    public Categoria? Categoria { get; set; }
    public ICollection<DetallePrestamo> DetallePrestamos { get; set; } = new List<DetallePrestamo>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
}
