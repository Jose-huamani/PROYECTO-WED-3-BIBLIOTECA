namespace Proyecto3Api.Domain.Entities;

public class PasswordResetToken
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresUtc { get; set; }
    public bool Usado { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
