namespace Magazzino.Shared;

public class ArticoloInsertDto
{
    public required string CodiceArticolo { get; set; }
    public int Quantita { get; set; }
}