namespace Proyecto3Api.Domain.Entities;

public class Favorito
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int LibroId { get; set; }
    public DateTime FechaAgregado { get; set; }

    public Usuario Usuario { get; set; } = null!;
    public Libro Libro { get; set; } = null!;
}
