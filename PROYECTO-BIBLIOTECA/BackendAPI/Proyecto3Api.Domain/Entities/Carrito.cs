namespace Proyecto3Api.Domain.Entities;

public class Carrito
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
    public ICollection<DetalleCarrito> Detalles { get; set; } = new List<DetalleCarrito>();
}
