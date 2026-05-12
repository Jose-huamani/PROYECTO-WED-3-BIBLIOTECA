namespace Proyecto3Api.Domain.Entities;

public class DetalleVenta
{
    public int Id { get; set; }
    public int VentaId { get; set; }
    public int LibroId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    public Venta Venta { get; set; } = null!;
    public Libro Libro { get; set; } = null!;
}
