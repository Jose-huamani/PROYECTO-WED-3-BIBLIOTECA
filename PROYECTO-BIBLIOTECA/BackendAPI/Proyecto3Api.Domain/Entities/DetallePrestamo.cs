namespace Proyecto3Api.Domain.Entities;

public class DetallePrestamo
{
    public int Id { get; set; }
    public int PrestamoId { get; set; }
    public int LibroId { get; set; }
    public int Cantidad { get; set; } = 1;

    public Prestamo Prestamo { get; set; } = null!;
    public Libro Libro { get; set; } = null!;
}
