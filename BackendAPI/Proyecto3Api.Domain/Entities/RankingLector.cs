namespace Proyecto3Api.Domain.Entities;

public class RankingLector
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int Puntos { get; set; }
    public DateTime UltimaActualizacion { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
