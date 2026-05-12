namespace Proyecto3Api.Domain.Entities;

public class Multa
{
    public int Id { get; set; }
    public int PrestamoId { get; set; }
    public int UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
    public bool Pagada { get; set; }
    public DateTime? FechaPago { get; set; }

    public Prestamo Prestamo { get; set; } = null!;
    public Usuario Usuario { get; set; } = null!;
}
