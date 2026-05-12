using System.ComponentModel.DataAnnotations;

namespace Proyecto3wed.Models;

public sealed class LoginFormModel
{
    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;
}

public sealed class RegisterFormModel
{
    [Required, MaxLength(200)]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "Mínimo 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Teléfono")]
    public string? Telefono { get; set; }

    [Display(Name = "Carrera")]
    public string? Carrera { get; set; }

    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }
}

public sealed class ForgotPasswordFormModel
{
    [Required, EmailAddress]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;
}

public sealed class ResetPasswordFormModel
{
    [Required]
    public string Token { get; set; } = string.Empty;

    [Required, MinLength(6)]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva contraseña")]
    public string NewPassword { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public sealed class ForgotPasswordResponseDto
{
    public string Mensaje { get; set; } = string.Empty;
    public string? ResetToken { get; set; }
}

public sealed class AuthResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
}

public sealed class AutorDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public sealed class CategoriaDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public sealed class LibroDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int AutorId { get; set; }
    public int CategoriaId { get; set; }
    public string CodigoQR { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public int CantidadTotal { get; set; }
    public int CantidadDisponible { get; set; }
    public bool EstaDisponible { get; set; }
    public string? ImagenUrl { get; set; }
    public string? Introduccion { get; set; }
    public string? Descripcion { get; set; }
    public string? Editorial { get; set; }
    public int NumeroPaginas { get; set; }
    public string? Idioma { get; set; }
    public string? EstadoLibro { get; set; }
    public AutorDto? Autor { get; set; }
    public CategoriaDto? Categoria { get; set; }
    public decimal Precio { get; set; }
}

public sealed class LibroMiniDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Autor { get; set; }
}

public sealed class LibroCreateForm
{
    [Required(ErrorMessage = "El titulo es obligatorio.")]
    [MaxLength(300, ErrorMessage = "El titulo no puede superar 300 caracteres.")]
    [Display(Name = "Nombre del libro")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El ISBN es obligatorio.")]
    [MaxLength(32, ErrorMessage = "El ISBN no puede superar 32 caracteres.")]
    [Display(Name = "ISBN")]
    public string Isbn { get; set; } = string.Empty;

    [Display(Name = "Autor existente")]
    public int AutorId { get; set; }

    [Required(ErrorMessage = "El autor es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El autor no puede superar 200 caracteres.")]
    [Display(Name = "Autor del libro")]
    public string AutorNombre { get; set; } = string.Empty;

    [Display(Name = "Categoria existente")]
    public int CategoriaId { get; set; }

    [Required(ErrorMessage = "La categoria es obligatoria.")]
    [MaxLength(150, ErrorMessage = "La categoria no puede superar 150 caracteres.")]
    [Display(Name = "Categoria o tipo del libro")]
    public string CategoriaNombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El codigo QR es obligatorio.")]
    [MaxLength(200, ErrorMessage = "El codigo QR es demasiado largo.")]
    [Display(Name = "Codigo QR")]
    public string CodigoQR { get; set; } = string.Empty;

    [MaxLength(200, ErrorMessage = "El codigo de barras es demasiado largo.")]
    [Display(Name = "Codigo de barras")]
    public string? CodigoBarras { get; set; }

    [Range(0, 100000, ErrorMessage = "Indica un valor entre 0 y 100000.")]
    [Display(Name = "Cantidad de stock")]
    public int CantidadTotal { get; set; } = 1;

    [Range(0, 100000, ErrorMessage = "Indica un valor entre 0 y 100000.")]
    [Display(Name = "Disponibles para préstamo")]
    public int CantidadDisponible { get; set; } = 1;

    [MaxLength(2048)]
    [Display(Name = "URL de la portada")]
    public string? ImagenUrl { get; set; }

    [Display(Name = "Fecha de ingreso")]
    public DateTime FechaIngreso { get; set; } = DateTime.Today;

    [MaxLength(2000)]
    [Display(Name = "Introduccion")]
    public string? Introduccion { get; set; }

    [MaxLength(500)]
    [Display(Name = "Descripcion")]
    public string? Descripcion { get; set; }

    [MaxLength(200)]
    [Display(Name = "Editorial")]
    public string? Editorial { get; set; }

    [Range(0, 100000)]
    [Display(Name = "Numero de paginas")]
    public int NumeroPaginas { get; set; }

    [MaxLength(80)]
    [Display(Name = "Idioma")]
    public string? Idioma { get; set; }

    [MaxLength(80)]
    [Display(Name = "Estado del libro")]
    public string? EstadoLibro { get; set; } = "Disponible";

    [Display(Name = "Precio en Bs")]
    public decimal Precio { get; set; }
}

public sealed class PrestamoLineForm
{
    public int LibroId { get; set; }
    public int Cantidad { get; set; } = 1;
}

public sealed class CrearPrestamoForm
{
    public int DiasPrestamo { get; set; } = 14;
    public int LibroId { get; set; }
    public int Cantidad { get; set; } = 1;
}

public sealed class DetallePrestamoDto
{
    public int Id { get; set; }
    public int LibroId { get; set; }
    public int Cantidad { get; set; }
    public LibroDto? Libro { get; set; }
}

public sealed class UsuarioMiniDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
}

public sealed class PrestamoDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime FechaPrestamo { get; set; }
    public DateTime FechaDevolucionEsperada { get; set; }
    public DateTime? FechaDevolucionReal { get; set; }
    public int Estado { get; set; }
    public UsuarioMiniDto? Usuario { get; set; }
    public List<DetallePrestamoDto> Detalles { get; set; } = new();
}

public sealed class FavoritoDto
{
    public int Id { get; set; }
    public int LibroId { get; set; }
    public DateTime FechaAgregado { get; set; }
    public LibroDto? Libro { get; set; }
}

public class ReservaDto
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int LibroId { get; set; }
    public DateTime FechaReserva { get; set; }
    public DateTime? FechaExpiracion { get; set; }
    public int Estado { get; set; }
    public string EstadoNombre { get; set; } = string.Empty;
    public LibroMiniDto? Libro { get; set; }
}

public sealed class ReservaAdminDto : ReservaDto
{
    public UsuarioMiniDto? Usuario { get; set; }
    public string? Observaciones { get; set; }
}

public sealed class NotificacionDto
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
}

public sealed class DashboardResumenDto
{
    public int TotalLibros { get; set; }
    public int EjemplaresDisponibles { get; set; }
    public int UsuariosActivos { get; set; }
    public int PrestamosActivos { get; set; }
    public int MultasPendientes { get; set; }
    public List<LibroPrestadoDto> LibrosMasPrestados { get; set; } = new();
    public List<TopLectorDto> TopLectores { get; set; } = new();
}

public sealed class LibroPrestadoDto
{
    public int LibroId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public int Veces { get; set; }
}

public sealed class TopLectorDto
{
    public int UsuarioId { get; set; }
    public int Puntos { get; set; }
}

public sealed class AdminUsuarioDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public bool Activo { get; set; }
    public int PrestamosActivos { get; set; }
    public int MultasPendientes { get; set; }
    public int Puntos { get; set; }
}

public sealed class UsuarioCreateForm
{
    [Required, MaxLength(200)]
    [Display(Name = "Nombre completo")]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required, EmailAddress]
    [Display(Name = "Correo electronico")]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(6)]
    [DataType(DataType.Password)]
    [Display(Name = "Contrasena temporal")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Rol")]
    public int RolId { get; set; } = 2;
}

public sealed class MultaDto
{
    public int Id { get; set; }
    public int PrestamoId { get; set; }
    public int UsuarioId { get; set; }
    public decimal Monto { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime FechaGeneracion { get; set; }
    public bool Pagada { get; set; }
    public DateTime? FechaPago { get; set; }
    public UsuarioMiniDto? Usuario { get; set; }
}

public sealed class AuditoriaDto
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public string Accion { get; set; } = string.Empty;
    public string TipoEntidad { get; set; } = string.Empty;
    public string? EntidadId { get; set; }
    public DateTime Fecha { get; set; }
    public string? IpAddress { get; set; }
}

public sealed class RankingDto
{
    public int UsuarioId { get; set; }
    public int Puntos { get; set; }
    public string Nombre { get; set; } = string.Empty;
}

public sealed class AdminModuleDto
{
    public string Titulo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Estado { get; set; } = "Activo";
    public string[] Funciones { get; set; } = Array.Empty<string>();
}

public sealed class VentaDto
{
    public int Id { get; set; }
    public string Usuario { get; set; } = string.Empty;
    public DateTime FechaVenta { get; set; }
    public decimal Total { get; set; }
    public string MetodoPago { get; set; } = string.Empty;
}

public sealed class VentaCreateRequest
{
    [Required]
    public string MetodoPago { get; set; } = string.Empty;
    public string? ReferenciaPago { get; set; }
}

public sealed class CarritoItemDto
{
    public int Id { get; set; }
    public int LibroId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal => PrecioUnitario * Cantidad;
}

public sealed class CarritoDto
{
    public int Id { get; set; }
    public List<CarritoItemDto> Items { get; set; } = new();
    public decimal Total => Items.Sum(i => i.Subtotal);
}

public sealed class SendNotificationRequest
{
    public int UsuarioId { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Mensaje { get; set; } = string.Empty;
    public string? Tipo { get; set; }
}
