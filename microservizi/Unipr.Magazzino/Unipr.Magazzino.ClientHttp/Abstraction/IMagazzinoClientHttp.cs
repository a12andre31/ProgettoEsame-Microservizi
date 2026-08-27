namespace Magazzino.ClientHttp.Abstraction;

public interface IMagazzinoClientHttp
{
    Task<bool> VerificaDisponibilitaAsync(string codiceArticolo, int quantitaRichiesta);
}