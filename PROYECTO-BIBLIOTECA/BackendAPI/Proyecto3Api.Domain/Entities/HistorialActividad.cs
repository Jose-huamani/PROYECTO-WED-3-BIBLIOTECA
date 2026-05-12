namespace Proyecto3Api.Domain.Entities;

public class HistorialActividad
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string TipoEvento { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int? LibroId { get; set; }
    public int? PrestamoId { get; set; }
    public DateTime Fecha { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
