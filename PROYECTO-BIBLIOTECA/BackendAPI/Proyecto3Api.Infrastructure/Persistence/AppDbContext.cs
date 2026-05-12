using Microsoft.EntityFrameworkCore;
using Proyecto3Api.Domain.Entities;

namespace Proyecto3Api.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Autor> Autores => Set<Autor>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Libro> Libros => Set<Libro>();
    public DbSet<Prestamo> Prestamos => Set<Prestamo>();
    public DbSet<DetallePrestamo> DetallePrestamos => Set<DetallePrestamo>();
    public DbSet<Multa> Multas => Set<Multa>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Favorito> Favoritos => Set<Favorito>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();
    public DbSet<Auditoria> Auditorias => Set<Auditoria>();
    public DbSet<RankingLector> Rankings => Set<RankingLector>();
    public DbSet<HistorialActividad> HistorialActividades => Set<HistorialActividad>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<DetalleVenta> DetalleVentas => Set<DetalleVenta>();
    public DbSet<Carrito> Carritos => Set<Carrito>();
    public DbSet<DetalleCarrito> DetalleCarritos => Set<DetalleCarrito>();
    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
            e.HasData(
                new Rol { Id = 1, Nombre = "Administrador" },
                new Rol { Id = 2, Nombre = "Lector" },
                new Rol { Id = 3, Nombre = "Bibliotecario" });
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.NombreCompleto).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Email).IsUnique();
            e.HasOne(x => x.Rol).WithMany(r => r.Usuarios).HasForeignKey(x => x.RolId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Autor>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        });

        modelBuilder.Entity<Categoria>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        });

        modelBuilder.Entity<Libro>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(300).IsRequired();
            e.Property(x => x.Isbn).HasMaxLength(32).IsRequired();
            e.Property(x => x.CodigoQR).HasMaxLength(200).IsRequired();
            e.Property(x => x.CodigoBarras).HasMaxLength(200);
            e.Property(x => x.Precio).HasPrecision(18, 2);
            e.HasIndex(x => x.CodigoQR).IsUnique();
            e.HasIndex(x => x.CodigoBarras).IsUnique().HasFilter("[CodigoBarras] IS NOT NULL");
            e.HasOne(x => x.Autor).WithMany(a => a.Libros).HasForeignKey(x => x.AutorId);
            e.HasOne(x => x.Categoria).WithMany(c => c.Libros).HasForeignKey(x => x.CategoriaId);
        });

        modelBuilder.Entity<Prestamo>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Estado).HasConversion<int>();
            e.HasOne(x => x.Usuario).WithMany(u => u.Prestamos).HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<DetallePrestamo>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Prestamo).WithMany(p => p.Detalles).HasForeignKey(x => x.PrestamoId);
            e.HasOne(x => x.Libro).WithMany(l => l.DetallePrestamos).HasForeignKey(x => x.LibroId);
        });

        modelBuilder.Entity<Multa>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Motivo).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.Prestamo).WithMany(p => p.Multas).HasForeignKey(x => x.PrestamoId);
            e.HasOne(x => x.Usuario).WithMany(u => u.Multas).HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<Reserva>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Estado).HasConversion<int>();
            e.HasOne(x => x.Usuario).WithMany(u => u.Reservas).HasForeignKey(x => x.UsuarioId);
            e.HasOne(x => x.Libro).WithMany(l => l.Reservas).HasForeignKey(x => x.LibroId);
        });

        modelBuilder.Entity<Favorito>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UsuarioId, x.LibroId }).IsUnique();
            e.HasOne(x => x.Usuario).WithMany(u => u.Favoritos).HasForeignKey(x => x.UsuarioId);
            e.HasOne(x => x.Libro).WithMany(l => l.Favoritos).HasForeignKey(x => x.LibroId);
        });

        modelBuilder.Entity<Notificacion>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            e.Property(x => x.Mensaje).HasMaxLength(2000).IsRequired();
            e.Property(x => x.Tipo).HasMaxLength(50).IsRequired();
            e.HasOne(x => x.Usuario).WithMany(u => u.Notificaciones).HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<Auditoria>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Accion).HasMaxLength(100).IsRequired();
            e.Property(x => x.TipoEntidad).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RankingLector>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UsuarioId).IsUnique();
            e.HasOne(x => x.Usuario).WithOne(u => u.Ranking).HasForeignKey<RankingLector>(x => x.UsuarioId);
        });

        modelBuilder.Entity<HistorialActividad>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TipoEvento).HasMaxLength(80).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(1000).IsRequired();
            e.HasOne(x => x.Usuario).WithMany(u => u.Historial).HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Token);
            e.HasOne(x => x.Usuario).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<PasswordResetToken>(e =>{
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Token).IsUnique();
            e.HasOne(x => x.Usuario).WithMany(u => u.PasswordResetTokens).HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<Prestamo>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Estado).HasConversion<int>();
            e.HasOne(x => x.Usuario).WithMany(u => u.Prestamos).HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<DetallePrestamo>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Prestamo).WithMany(p => p.Detalles).HasForeignKey(x => x.PrestamoId);
            e.HasOne(x => x.Libro).WithMany(l => l.DetallePrestamos).HasForeignKey(x => x.LibroId);
        });

        modelBuilder.Entity<Multa>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Motivo).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.Prestamo).WithMany(p => p.Multas).HasForeignKey(x => x.PrestamoId);
            e.HasOne(x => x.Usuario).WithMany(u => u.Multas).HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<Reserva>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Estado).HasConversion<int>();
            e.HasOne(x => x.Usuario).WithMany(u => u.Reservas).HasForeignKey(x => x.UsuarioId);
            e.HasOne(x => x.Libro).WithMany(l => l.Reservas).HasForeignKey(x => x.LibroId);
        });

        modelBuilder.Entity<Favorito>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.UsuarioId, x.LibroId }).IsUnique();
            e.HasOne(x => x.Usuario).WithMany(u => u.Favoritos).HasForeignKey(x => x.UsuarioId);
            e.HasOne(x => x.Libro).WithMany(l => l.Favoritos).HasForeignKey(x => x.LibroId);
        });

        modelBuilder.Entity<Notificacion>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(200).IsRequired();
            e.Property(x => x.Mensaje).HasMaxLength(2000).IsRequired();
            e.Property(x => x.Tipo).HasMaxLength(50).IsRequired();
            e.HasOne(x => x.Usuario).WithMany(u => u.Notificaciones).HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<Auditoria>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Accion).HasMaxLength(100).IsRequired();
            e.Property(x => x.TipoEntidad).HasMaxLength(100).IsRequired();
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId).OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<RankingLector>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.UsuarioId).IsUnique();
            e.HasOne(x => x.Usuario).WithOne(u => u.Ranking).HasForeignKey<RankingLector>(x => x.UsuarioId);
        });

        modelBuilder.Entity<HistorialActividad>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TipoEvento).HasMaxLength(80).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(1000).IsRequired();
            e.HasOne(x => x.Usuario).WithMany(u => u.Historial).HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Token);
            e.HasOne(x => x.Usuario).WithMany(u => u.RefreshTokens).HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<PasswordResetToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Token).HasMaxLength(500).IsRequired();
            e.HasIndex(x => x.Token).IsUnique();
            e.HasOne(x => x.Usuario).WithMany(u => u.PasswordResetTokens).HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<Venta>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Total).HasPrecision(18, 2);
            e.Property(x => x.MetodoPago).HasMaxLength(50).IsRequired();
            e.Property(x => x.ReferenciaPago).HasMaxLength(100);
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<DetalleVenta>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.PrecioUnitario).HasPrecision(18, 2);
            e.HasOne(x => x.Venta).WithMany(v => v.Detalles).HasForeignKey(x => x.VentaId);
            e.HasOne(x => x.Libro).WithMany().HasForeignKey(x => x.LibroId);
        });

        modelBuilder.Entity<Carrito>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId);
        });

        modelBuilder.Entity<DetalleCarrito>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Carrito).WithMany(c => c.Detalles).HasForeignKey(x => x.CarritoId);
            e.HasOne(x => x.Libro).WithMany().HasForeignKey(x => x.LibroId);
        });

        modelBuilder.Entity<DeviceToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.FcmToken).HasMaxLength(500).IsRequired();
            e.Property(x => x.Device).HasMaxLength(200);
            e.HasOne(x => x.Usuario).WithMany(u => u.DeviceTokens).HasForeignKey(x => x.UsuarioId);
        });
    }
}

