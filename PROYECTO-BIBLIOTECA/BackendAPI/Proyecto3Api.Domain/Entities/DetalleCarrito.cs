namespace Proyecto3Api.Domain.Entities;

public class DetalleCarrito
{
    public int Id { get; set; }
    public int CarritoId { get; set; }
    public int LibroId { get; set; }
    public int Cantidad { get; set; }

    public Carrito Carrito { get; set; } = null!;
    public Libro Libro { get; set; } = null!;
}
