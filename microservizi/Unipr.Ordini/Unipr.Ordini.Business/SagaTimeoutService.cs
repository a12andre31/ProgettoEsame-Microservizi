using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Ordini.Business.Abstraction;
using Ordini.Business.Factory;
using Ordini.Repository.Abstraction;
using Ordini.Shared;
using AutoMapper;

namespace Ordini.Business;

public class SagaTimeoutService(
    ILogger<SagaTimeoutService> logger,
    IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            logger.LogInformation("Controllo SAGA Timeout in corso...");

            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
                var observer = scope.ServiceProvider.GetRequiredService<IUniprOrdiniObserver>();
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();

                // Chiediamo al DB gli ordini fermi da più di 5 minuti
                var ordiniScaduti = await repository.GetOrdiniScadutiAsync(5, stoppingToken);

                if (ordiniScaduti.Any())
                {
                    foreach (var ordine in ordiniScaduti)
                    {
                        logger.LogWarning("Ordine {Id} bloccato in stato {Stato}. Avvio Rollback!", ordine.Id, ordine.Stato);

                        // Apriamo la transazione per annullare l'ordine e avvisare Kafka
                        await repository.BeginTransactionAsync(async (cancellation) =>
                        {
                            string statoAnnullamento = ordine.Stato == "Pending" ? "Timeout_Magazzino" : "Timeout_Pagamenti";
                            var ordineAnnullato = await repository.UpdateStatoOrdineAsync(ordine.Id, statoAnnullamento, cancellation);

                            // Trasformiamo in DTO per inviarlo via Kafka
                            var ordineDto = map.Map<OrdineReadDto>(ordineAnnullato);

                            // Usiamo CreateUpdate per notificare a tutti il cambio di stato
                            var outboxMessage = TransactionalOutboxFactory.CreateUpdate(ordineDto);

                            await repository.InsertTransactionalOutboxAsync(outboxMessage, cancellation);
                            await repository.SaveChangesAsync(cancellation);

                        }, stoppingToken);

                        // Spediamo il rollback
                        observer.NuovoOrdine.OnNext(1);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Errore durante l'esecuzione del SagaTimeoutService.");
            }

            // Mettiamo a dormire il servizio per 1 minuto prima del prossimo controllo
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}