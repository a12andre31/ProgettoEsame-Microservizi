namespace Ordini.Shared;

public class OrdineInsertDto
{
    public required string CodiceArticolo { get; set; }
    public int Quantita { get; set; }
    public decimal PrezzoTotale { get; set; }
    public int IdCliente { get; set; }
}