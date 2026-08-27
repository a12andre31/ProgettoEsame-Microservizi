namespace Magazzino.Shared;

public class RispostaMagazzinoDto
{
    public int IdOrdine { get; set; }
    public required string Esito { get; set; } // Sarà "Confermato" o "Rifiutato"
}