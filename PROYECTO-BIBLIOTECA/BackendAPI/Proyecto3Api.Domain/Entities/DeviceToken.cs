namespace Proyecto3Api.Domain.Entities;

public class DeviceToken
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string FcmToken { get; set; } = string.Empty;
    public string? Device { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Usuario Usuario { get; set; } = null!;
}
