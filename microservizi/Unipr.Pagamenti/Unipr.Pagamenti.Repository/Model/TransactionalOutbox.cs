namespace Pagamenti.Repository.Model;

public class TransactionalOutbox
{
    public long Id { get; set; }
    public string Tabella { get; set; } = string.Empty;
    public string Messaggio { get; set; } = string.Empty;
}