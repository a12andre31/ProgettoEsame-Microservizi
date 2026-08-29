namespace Ordini.ClientHttp.Abstraction;

public interface IOrdiniClientHttp
{
   
    Task<string?> GetStatoOrdineAsync(int idOrdine);
}