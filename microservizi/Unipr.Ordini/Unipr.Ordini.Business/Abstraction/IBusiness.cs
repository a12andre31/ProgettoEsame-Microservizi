using Ordini.Shared;

namespace Ordini.Business.Abstraction;

public interface IBusiness
{
    Task CreateOrdineAsync(OrdineInsertDto ordineInsertDto, CancellationToken cancellationToken = default);
}