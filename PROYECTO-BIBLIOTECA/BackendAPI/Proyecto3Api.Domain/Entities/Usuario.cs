namespace Proyecto3Api.Domain.Entities;

public class Usuario
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public int RolId { get; set; }
    public string? FotoPerfilUrl { get; set; }
    public bool Activo { get; set; } = true;

    public Rol Rol { get; set; } = null!;
    public ICollection<Prestamo> Prestamos { get; set; } = new List<Prestamo>();
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public ICollection<Favorito> Favoritos { get; set; } = new List<Favorito>();
    public ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();
    public ICollection<HistorialActividad> Historial { get; set; } = new List<HistorialActividad>();
    public RankingLector? Ranking { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Multa> Multas { get; set; } = new List<Multa>();
    public ICollection<PasswordResetToken> PasswordResetTokens { get; set; } = new List<PasswordResetToken>();
    public ICollection<DeviceToken> DeviceTokens { get; set; } = new List<DeviceToken>();
    public string? Telefono { get; set; }
    public string? Carrera { get; set; }
    public string? Direccion { get; set; }
}
