using Magazzino.Business.Abstraction;
using Magazzino.Business.Factory;
using Magazzino.Repository.Abstraction;
using Magazzino.Shared;

namespace Magazzino.Business;

public class Business(IRepository repository, IUniprMagazzinoObserver observer) : IBusiness
{
    // Questo metodo serve per riempire il magazzino all'inizio da Swagger
    public async Task RifornisciMagazzinoAsync(ArticoloInsertDto dto, CancellationToken cancellationToken = default)
    {
        await repository.BeginTransactionAsync(async (cancellation) =>
        {
            await repository.CreateOrUpdateArticoloAsync(dto, cancellation);
            await repository.SaveChangesAsync(cancellation);
        }, cancellationToken);
    }

    public async Task ElaboraPrenotazioneAsync(int idOrdine, string codiceArticolo, int quantitaRichiesta, CancellationToken cancellationToken = default)
    {
        await repository.BeginTransactionAsync(async (cancellation) =>
        {
            var articolo = await repository.GetArticoloByCodiceAsync(codiceArticolo, cancellation);
            bool esitoPositivo = false;

            // Controlliamo se c'è abbastanza merce
            if (articolo != null && articolo.QuantitaDisponibile >= quantitaRichiesta)
            {
                // Se sì, scalo la quantità
                await repository.CreateOrUpdateArticoloAsync(new ArticoloInsertDto { CodiceArticolo = codiceArticolo, Quantita = -quantitaRichiesta }, cancellation);
                esitoPositivo = true;
            }

            // Creo la risposta e la metto in Outbox
            var risposta = new RispostaMagazzinoDto { IdOrdine = idOrdine, Esito = esitoPositivo ? "Confermato" : "Rifiutato" };

            await repository.InsertTransactionalOutboxAsync(TransactionalOutboxFactory.CreateInsert(risposta), cancellation);
            await repository.SaveChangesAsync(cancellation);

        }, cancellationToken);

        // Sveglia per mandare la risposta a Kafka
        observer.NuovoOrdine.OnNext(1);
    }

    public async Task<bool> VerificaDisponibilitaAsync(string codiceArticolo, int quantitaRichiesta, CancellationToken cancellationToken = default)
    {
        // Chiede al database se l'articolo esiste e se ce n'è abbastanza
        var articolo = await repository.GetArticoloByCodiceAsync(codiceArticolo, cancellationToken);
        return articolo != null && articolo.QuantitaDisponibile >= quantitaRichiesta;
    }
}