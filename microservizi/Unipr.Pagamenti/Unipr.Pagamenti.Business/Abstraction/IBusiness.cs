namespace Pagamenti.Business.Abstraction;

public interface IBusiness
{
    Task RicaricaContoAsync(int idCliente, decimal importo, CancellationToken cancellationToken = default);
    Task ElaboraPagamentoAsync(int idOrdine, int idCliente, decimal importo, CancellationToken cancellationToken = default);
    Task RimborsaPagamentoAsync(int idCliente, decimal importo, CancellationToken cancellationToken = default);
}