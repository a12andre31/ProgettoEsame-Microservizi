using System.Net.Http.Json;
using Magazzino.ClientHttp.Abstraction;

namespace Magazzino.ClientHttp;

public class MagazzinoClientHttp(HttpClient httpClient) : IMagazzinoClientHttp
{
    public async Task<bool> VerificaDisponibilitaAsync(string codiceArticolo, int quantitaRichiesta)
    {
        // Facciamo una chiamata HTTP GET all'indirizzo del Magazzino
        var response = await httpClient.GetAsync($"/Magazzino/Verifica?codiceArticolo={codiceArticolo}&quantita={quantitaRichiesta}");

        if (response.IsSuccessStatusCode)
        {
            // Se risponde 200 OK, leggiamo se ci ha detto true o false
            return await response.Content.ReadFromJsonAsync<bool>();
        }

        return false; // In caso di errore o se non lo trova, diamo false
    }
}