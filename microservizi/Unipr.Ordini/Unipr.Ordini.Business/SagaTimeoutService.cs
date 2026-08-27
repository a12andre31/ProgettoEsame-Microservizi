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

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
                var observer = scope.ServiceProvider.GetRequiredService<IUniprOrdiniObserver>();
                var map = scope.ServiceProvider.GetRequiredService<IMapper>();

                //simuliamo il controllo: cerchiamo ordini Pending vecchi di 5 minuti.
                // Se ne troviamo, li annulliamo e notifichiamo Kafka.
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}