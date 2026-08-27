namespace Ordini.Shared;

public class OrdineReadDto
{
    public int Id { get; set; }
    public required string CodiceArticolo { get; set; }
    public int Quantita { get; set; }
    public decimal PrezzoTotale { get; set; }
    public int IdCliente { get; set; }
    public required string Stato { get; set; } //"Pending", "Completed", "Canceled"
    public DateTime DataCreazione { get; set; }
}