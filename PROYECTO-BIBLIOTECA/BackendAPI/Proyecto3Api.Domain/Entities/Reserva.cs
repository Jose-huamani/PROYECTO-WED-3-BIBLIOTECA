using Proyecto3Api.Domain.Enums;

namespace Proyecto3Api.Domain.Entities;

public class Reserva
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int LibroId { get; set; }
    public DateTime FechaReserva { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public ReservaEstado Estado { get; set; } = ReservaEstado.Pendiente;

    public Usuario Usuario { get; set; } = null!;
    public Libro Libro { get; set; } = null!;
}
