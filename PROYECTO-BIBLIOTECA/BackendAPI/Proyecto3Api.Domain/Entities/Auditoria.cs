namespace Proyecto3Api.Domain.Entities;

public class Auditoria
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string TipoEntidad { get; set; } = string.Empty;
    public string? EntidadId { get; set; }
    public string? Detalle { get; set; }
    public string? IpAddress { get; set; }
    public DateTime Fecha { get; set; }

    public Usuario? Usuario { get; set; }
}
