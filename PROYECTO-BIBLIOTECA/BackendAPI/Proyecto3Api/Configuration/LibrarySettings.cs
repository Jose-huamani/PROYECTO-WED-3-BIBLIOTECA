namespace Proyecto3Api.Configuration;

public sealed class LibrarySettings
{
    public const string SectionName = "Library";

    public decimal MultaPorDiaRetraso { get; set; } = 2;
    public int PuntosDevolucionPuntual { get; set; } = 10;
}
