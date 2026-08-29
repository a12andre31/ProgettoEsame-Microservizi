namespace Ordini.Shared;

public class OrdineDaPagareDto
{
    public int IdOrdine { get; set; }
    public decimal Importo { get; set; }
    public int IdCliente { get; set; }
}