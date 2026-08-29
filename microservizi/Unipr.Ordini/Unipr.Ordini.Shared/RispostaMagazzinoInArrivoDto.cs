namespace Ordini.Shared;

public class RispostaMagazzinoInArrivoDto
{
    public int IdOrdine { get; set; }
    public required string Esito { get; set; }
}