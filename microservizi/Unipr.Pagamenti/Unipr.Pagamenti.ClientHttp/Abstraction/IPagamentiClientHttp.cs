namespace Pagamenti.ClientHttp.Abstraction;

public interface IPagamentiClientHttp
{
    Task<decimal?> GetSaldoClienteAsync(int idCliente);
}