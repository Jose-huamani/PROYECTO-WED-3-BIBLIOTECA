using Proyecto3Api.Domain.Enums;

namespace Proyecto3Api.Domain.Entities;

public class Prestamo
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucionEsperada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    public PrestamoEstado Estado { get; set; } = PrestamoEstado.Activo;

    public Usuario Usuario { get; set; } = null!;
    public ICollection<DetallePrestamo> Detalles { get; set; } = new List<DetallePrestamo>();
    public ICollection<Multa> Multas { get; set; } = new List<Multa>();
}
