namespace Magazzino.Repository.Model;

public class Articolo
{
    public int Id { get; set; }
    public required string CodiceArticolo { get; set; }
    public int QuantitaDisponibile { get; set; }
}