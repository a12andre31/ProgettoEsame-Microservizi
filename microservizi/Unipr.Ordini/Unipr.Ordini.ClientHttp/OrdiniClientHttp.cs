using System.Net.Http.Json;
using Ordini.ClientHttp.Abstraction;

namespace Ordini.ClientHttp;

public class OrdiniClientHttp(HttpClient httpClient) : IOrdiniClientHttp
{
    public async Task<string?> GetStatoOrdineAsync(int idOrdine)
    {
        var response = await httpClient.GetAsync($"/Ordini/Stato?id={idOrdine}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        return null;
    }
}