namespace Pagamenti.Shared;

public class OrdineAnnullatoDto
{
    public int IdCliente { get; set; }
    public decimal PrezzoTotale { get; set; }
    public string Stato { get; set; } = string.Empty;
}