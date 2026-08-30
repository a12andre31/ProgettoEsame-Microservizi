namespace Ordini.Shared;

public class RispostaPagamentoInArrivoDto
{
    public int IdOrdine { get; set; }
    public required string Esito { get; set; }
}