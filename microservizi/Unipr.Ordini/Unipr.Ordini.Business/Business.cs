using AutoMapper;
using Magazzino.ClientHttp.Abstraction;
using Microsoft.Extensions.Logging;
using Ordini.Business.Abstraction;
using Ordini.Business.Factory;
using Ordini.Repository.Abstraction;
using Ordini.Shared;

namespace Ordini.Business;

public class Business(IRepository repository, ILogger<Business> logger, IMapper map, IUniprOrdiniObserver observer , IMagazzinoClientHttp magazzinoClient) : IBusiness
{
    public async Task CreateOrdineAsync(OrdineInsertDto ordineInsertDto, CancellationToken cancellationToken = default)
    {
        //CHIAMATA HTTP AL MAGAZZINO (Prima di toccare il Database)
        bool isDisponibile = await magazzinoClient.VerificaDisponibilitaAsync(
            ordineInsertDto.CodiceArticolo,
            ordineInsertDto.Quantita);

        // Se il magazzino risponde picche, blocchiamo l'ordine e lanciamo un errore
        if (!isDisponibile)
        {
            throw new Exception($"Impossibile creare l'ordine: L'articolo '{ordineInsertDto.CodiceArticolo}' non è disponibile in magazzino per la quantità richiesta ({ordineInsertDto.Quantita}).");
        }

        // 2. Se c'è disponibilità, procediamo normalmente salvando l'ordine
        // Avviamo la transazione sicura
        await repository.BeginTransactionAsync(async (CancellationToken cancellation) =>
        {
            // Salviamo l'ordine sul DB
            var ordine = await repository.CreateOrdineAsync(ordineInsertDto, cancellation);
            await repository.SaveChangesAsync(cancellation); // Salvataggio intermedio per fargli generare l'Id

            // Travasiamo il model sul DTO di lettura
            var newOrdineRecord = map.Map<OrdineReadDto>(ordine);

            // Creiamo il messaggio per Kafka e lo inseriamo nella tabella TransactionalOutboxList
            await repository.InsertTransactionalOutboxAsync(TransactionalOutboxFactory.CreateInsert(newOrdineRecord), cancellation);
            await repository.SaveChangesAsync(cancellation); // Conferma definitiva

        }, cancellationToken);

        // Notifichiamo in tempo reale al servizio in background che c'è un nuovo messaggio da spedire
        observer.NuovoOrdine.OnNext(1);
    }

    public async Task GestisciRispostaMagazzinoAsync(int idOrdine, string esito, CancellationToken cancellationToken = default)
    {
        await repository.BeginTransactionAsync(async (cancellation) =>
        {
            string nuovoStato = esito == "Confermato" ? "PendingPayment" : "Canceled";

            // Salviamo lo stato e ci facciamo restituire l'ordine per leggere Importo e IdCliente
            var ordine = await repository.UpdateStatoOrdineAsync(idOrdine, nuovoStato, cancellation);
            await repository.SaveChangesAsync(cancellation);

            // Se passiamo alla fase di pagamento, prepariamo la lettera per Kafka
            if (nuovoStato == "PendingPayment" && ordine != null)
            {
                var richiesta = new OrdineDaPagareDto
                {
                    IdOrdine = ordine.Id,
                    Importo = ordine.PrezzoTotale,
                    IdCliente = ordine.IdCliente
                };

                await repository.InsertTransactionalOutboxAsync(TransactionalOutboxFactory.CreatePaymentRequest(richiesta), cancellation);
                await repository.SaveChangesAsync(cancellation);
            }
        }, cancellationToken);

        // Suoniamo il campanello per spedire tutto!
        observer.NuovoOrdine.OnNext(1);
    }

    public async Task GestisciRispostaPagamentoAsync(int idOrdine, string esito, CancellationToken cancellationToken = default)
    {
        await repository.BeginTransactionAsync(async (cancellation) =>
        {
            string nuovoStato = esito == "Pagato" ? "Completed" : "Canceled";

            var ordine = await repository.UpdateStatoOrdineAsync(idOrdine, nuovoStato, cancellation);

            // Transazione di Compensazione (Rollback) se il pagamento fallisce
            if (nuovoStato == "Canceled" && ordine != null)
            {
                var dtoAnnullamento = map.Map<OrdineReadDto>(ordine);
                var outboxMessage = TransactionalOutboxFactory.CreateUpdate(dtoAnnullamento);
                await repository.InsertTransactionalOutboxAsync(outboxMessage, cancellation);
            }

            await repository.SaveChangesAsync(cancellation);
        }, cancellationToken);

        observer.NuovoOrdine.OnNext(1);
    }
}