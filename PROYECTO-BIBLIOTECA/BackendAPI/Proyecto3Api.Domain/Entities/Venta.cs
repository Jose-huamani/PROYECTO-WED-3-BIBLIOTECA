namespace Proyecto3Api.Domain.Entities;

public class Venta
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaVenta { get; set; }
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
    public string ReferenciaPago { get; set; } = string.Empty;

    public Usuario Usuario { get; set; } = null!;
    public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
}
