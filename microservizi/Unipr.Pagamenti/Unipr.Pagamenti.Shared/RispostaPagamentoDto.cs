namespace Pagamenti.Shared;

public class RispostaPagamentoDto
{
    public int IdOrdine { get; set; }
    public required string Esito { get; set; } // "Pagato" o "Rifiutato"
}