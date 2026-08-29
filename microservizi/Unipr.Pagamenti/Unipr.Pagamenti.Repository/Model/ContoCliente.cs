namespace Pagamenti.Repository.Model;

public class ContoCliente
{
    public int Id { get; set; }
    public int IdCliente { get; set; }
    public decimal Saldo { get; set; }
}