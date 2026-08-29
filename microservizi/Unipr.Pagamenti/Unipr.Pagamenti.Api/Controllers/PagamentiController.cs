using Microsoft.AspNetCore.Mvc;
using Pagamenti.Business.Abstraction;

namespace Pagamenti.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class PagamentiController(IBusiness business) : ControllerBase
{
    [HttpPost(Name = "RicaricaConto")]
    public async Task<ActionResult> RicaricaConto([FromQuery] int idCliente, [FromQuery] decimal importo)
    {
        await business.RicaricaContoAsync(idCliente, importo);
        return Ok($"Conto del cliente {idCliente} ricaricato di {importo} euro con successo!");
    }
}