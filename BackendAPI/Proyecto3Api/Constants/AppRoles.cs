namespace Proyecto3Api.Constants;

public static class AppRoles
{
    public const string Administrador = "Administrador";
    public const string Lector = "Lector";
    public const string Bibliotecario = "Bibliotecario";

    public const string AdminOBibliotecario = Administrador + "," + Bibliotecario;
}
