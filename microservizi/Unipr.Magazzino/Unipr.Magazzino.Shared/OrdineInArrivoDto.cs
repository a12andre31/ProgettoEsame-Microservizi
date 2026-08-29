namespace Magazzino.Shared;

public class OrdineInArrivoDto
{
    public int Id { get; set; }
    public required string CodiceArticolo { get; set; }
    public int Quantita { get; set; }
    public required string Stato { get; set; }
}