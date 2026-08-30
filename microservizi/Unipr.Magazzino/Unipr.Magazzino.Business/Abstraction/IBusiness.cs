using Magazzino.Shared;

namespace Magazzino.Business.Abstraction;

public interface IBusiness
{
    Task RifornisciMagazzinoAsync(ArticoloInsertDto ordineInsertDto, CancellationToken cancellationToken = default);
    Task<bool> VerificaDisponibilitaAsync(string codiceArticolo, int quantitaRichiesta, CancellationToken cancellationToken = default);
    Task ElaboraPrenotazioneAsync(int idOrdine, string codiceArticolo, int quantitaRichiesta, CancellationToken cancellationToken = default);

    Task AnnullaPrenotazioneAsync(string codiceArticolo, int quantita, CancellationToken cancellationToken = default);
}