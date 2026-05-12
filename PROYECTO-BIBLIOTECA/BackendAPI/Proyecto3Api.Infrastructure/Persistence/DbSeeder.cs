using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Proyecto3Api.Domain.Entities;
using Proyecto3Api.Infrastructure.Security;

namespace Proyecto3Api.Infrastructure.Persistence;

public static class DbSeeder
{
    private const int RolLector = 2;

    public static async Task SeedAsync(AppDbContext db, IConfiguration configuration)
    {
        await EnsureRolesAsync(db);
        await EnsureAdministradorAsync(db, configuration);
        await EnsureLectoresDemoAsync(db, configuration);
        await EnsureLibrosDemoAsync(db, configuration);
    }

    private static async Task EnsureRolesAsync(AppDbContext db)
    {
        var roles = new[]
        {
            new Rol { Id = 1, Nombre = "Administrador" },
            new Rol { Id = 2, Nombre = "Lector" },
            new Rol { Id = 3, Nombre = "Bibliotecario" }
        };

        foreach (var rol in roles)
        {
            if (!await db.Roles.AnyAsync(r => r.Id == rol.Id || r.Nombre == rol.Nombre))
            {
                db.Roles.Add(rol);
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task EnsureAdministradorAsync(AppDbContext db, IConfiguration configuration)
    {
        var email = configuration["Seed:AdminEmail"]?.Trim().ToLowerInvariant();
        var password = configuration["Seed:AdminPassword"];
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            return;
        }

        if (await db.Usuarios.AnyAsync(u => u.Email == email))
        {
            return;
        }

        var admin = new Usuario
        {
            NombreCompleto = "Administrador",
            Email = email,
            PasswordHash = PasswordHasher.Hash(password),
            RolId = 1,
            Activo = true
        };

        db.Usuarios.Add(admin);
        await db.SaveChangesAsync();

        db.Rankings.Add(new RankingLector
        {
            UsuarioId = admin.Id,
            Puntos = 0,
            UltimaActualizacion = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
    }

    private static async Task EnsureLectoresDemoAsync(AppDbContext db, IConfiguration configuration)
    {
        foreach (var item in configuration.GetSection("Seed:Lectores").GetChildren())
        {
            var nombre = item["NombreCompleto"];
            var email = item["Email"]?.Trim().ToLowerInvariant();
            var password = item["Password"];
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password))
            {
                continue;
            }

            if (await db.Usuarios.AnyAsync(u => u.Email == email))
            {
                continue;
            }

            var usuario = new Usuario
            {
                NombreCompleto = nombre.Trim(),
                Email = email,
                PasswordHash = PasswordHasher.Hash(password),
                RolId = RolLector,
                Activo = true
            };

            db.Usuarios.Add(usuario);
            await db.SaveChangesAsync();

            db.Rankings.Add(new RankingLector
            {
                UsuarioId = usuario.Id,
                Puntos = 0,
                UltimaActualizacion = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
    }

    private static async Task EnsureLibrosDemoAsync(AppDbContext db, IConfiguration configuration)
    {
        if (!configuration.GetValue("Seed:IncludeSampleBooks", false))
        {
            return;
        }

        if (await db.Libros.AnyAsync())
        {
            return;
        }

        var catFiccion = new Categoria { Nombre = "Ficción" };
        var catInfantil = new Categoria { Nombre = "Infantil" };
        var catTec = new Categoria { Nombre = "Programación y software" };
        var catHistoria = new Categoria { Nombre = "Historia" };
        var catDivulgacion = new Categoria { Nombre = "Divulgación" };

        db.Categorias.AddRange(catFiccion, catInfantil, catTec, catHistoria, catDivulgacion);

        var aGarcia = new Autor { Nombre = "Gabriel García Márquez" };
        var aSaint = new Autor { Nombre = "Antoine de Saint-Exupéry" };
        var aMartin = new Autor { Nombre = "Robert C. Martin" };
        var aGarciaHistoria = new Autor { Nombre = "Fernando García de Cortázar" };
        var aHarari = new Autor { Nombre = "Yuval Noah Harari" };

        db.Autores.AddRange(aGarcia, aSaint, aMartin, aGarciaHistoria, aHarari);
        await db.SaveChangesAsync();

        void AddLibro(string titulo, string isbn, int autorId, int catId, string qr, string? barras, int total)
        {
            db.Libros.Add(new Libro
            {
                Titulo = titulo,
                Isbn = isbn,
                AutorId = autorId,
                CategoriaId = catId,
                CodigoQR = qr,
                CodigoBarras = barras,
                CantidadTotal = total,
                CantidadDisponible = total
            });
        }

        AddLibro(
            "Cien años de soledad",
            "9780307476464",
            aGarcia.Id,
            catFiccion.Id,
            "LIB-DEMO-QR-0001",
            "7501234567890",
            3);

        AddLibro(
            "El principito",
            "9788498381498",
            aSaint.Id,
            catInfantil.Id,
            "LIB-DEMO-QR-0002",
            "7501234567891",
            5);

        AddLibro(
            "Clean Code",
            "9780132350884",
            aMartin.Id,
            catTec.Id,
            "LIB-DEMO-QR-0003",
            "7501234567892",
            2);

        AddLibro(
            "Breve historia de España",
            "9788408177665",
            aGarciaHistoria.Id,
            catHistoria.Id,
            "LIB-DEMO-QR-0004",
            null,
            4);

        AddLibro(
            "Sapiens",
            "9788499926223",
            aHarari.Id,
            catDivulgacion.Id,
            "LIB-DEMO-QR-0005",
            "7501234567894",
            3);

        await db.SaveChangesAsync();
    }
}
