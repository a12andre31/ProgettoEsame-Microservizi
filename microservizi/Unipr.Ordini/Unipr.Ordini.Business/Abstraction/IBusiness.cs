using Ordini.Shared;

namespace Ordini.Business.Abstraction;

public interface IBusiness
{
    Task CreateOrdineAsync(OrdineInsertDto ordineInsertDto, CancellationToken cancellationToken = default);

    Task GestisciRispostaMagazzinoAsync(int idOrdine, string esito, CancellationToken cancellationToken = default);

    Task GestisciRispostaPagamentoAsync(int idOrdine, string esito, CancellationToken cancellationToken = default);
}