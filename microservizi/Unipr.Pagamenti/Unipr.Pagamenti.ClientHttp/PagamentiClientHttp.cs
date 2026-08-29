using System.Net.Http.Json;
using Pagamenti.ClientHttp.Abstraction;

namespace Pagamenti.ClientHttp;

public class PagamentiClientHttp(HttpClient httpClient) : IPagamentiClientHttp
{
    public async Task<decimal?> GetSaldoClienteAsync(int idCliente)
    {
        
        var response = await httpClient.GetAsync($"/Pagamenti/Saldo?idCliente={idCliente}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<decimal>();
        }

        return null;
    }
}