using Pagamenti.Business.Abstraction;
using Pagamenti.Business.Factory;
using Pagamenti.Repository.Abstraction;
using Pagamenti.Shared;

namespace Pagamenti.Business;

public class Business(IRepository repository, IUniprPagamentiObserver observer) : IBusiness
{
    // Metodo di servizio per aggiungere soldi da Swagger all'avvio
    public async Task RicaricaContoAsync(int idCliente, decimal importo, CancellationToken cancellationToken = default)
    {
        await repository.BeginTransactionAsync(async (cancellation) =>
        {
            await repository.CreaOAggiornaContoAsync(idCliente, importo, cancellation);
            await repository.SaveChangesAsync(cancellation);
        }, cancellationToken);
    }

    // IL PERNO DELLA SAGA (Transazione Pivot)
    public async Task ElaboraPagamentoAsync(int idOrdine, int idCliente, decimal importo, CancellationToken cancellationToken = default)
    {
        await repository.BeginTransactionAsync(async (cancellation) =>
        {
            // 1. Tenta il prelievo. Se true, la SAGA è tecnicamente conclusa con successo.
            bool esitoPositivo = await repository.PrelevaFondiAsync(idCliente, importo, cancellation);

            // 2. Prepara il verdetto
            var risposta = new RispostaPagamentoDto
            {
                IdOrdine = idOrdine,
                Esito = esitoPositivo ? "Pagato" : "Rifiutato_FondiInsufficienti"
            };

            // 3. Imbuca la lettera per avvisare Ordini e Magazzino
            await repository.InsertTransactionalOutboxAsync(TransactionalOutboxFactory.CreateInsert(risposta), cancellation);
            await repository.SaveChangesAsync(cancellation);

        }, cancellationToken);

        // 4. Suona il campanello per spedire il messaggio Kafka
        observer.NuovoPagamento.OnNext(1);
    }

    public async Task RimborsaPagamentoAsync(int idCliente, decimal importo, CancellationToken cancellationToken = default)
    {
        await repository.BeginTransactionAsync(async (cancellation) =>
        {
            await repository.CreaOAggiornaContoAsync(idCliente, importo, cancellation);
            await repository.SaveChangesAsync(cancellation);
        }, cancellationToken);
    }
}