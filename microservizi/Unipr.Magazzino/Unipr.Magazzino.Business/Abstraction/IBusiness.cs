using Magazzino.Shared;

namespace Magazzino.Business.Abstraction;

public interface IBusiness
{
    Task RifornisciMagazzinoAsync(ArticoloInsertDto ordineInsertDto, CancellationToken cancellationToken = default);

    Task<bool> VerificaDisponibilitaAsync(string codiceArticolo, int quantitaRichiesta, CancellationToken cancellationToken = default);
}