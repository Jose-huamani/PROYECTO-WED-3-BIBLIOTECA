namespace Proyecto3wed.Options;

public class BibliotecaApiOptions
{
    public const string SectionName = "BibliotecaApi";

    /// <summary>URL base de la API (sin barra final). Ej: https://localhost:7223</summary>
    public string BaseUrl { get; set; } = "https://localhost:7223";
}
