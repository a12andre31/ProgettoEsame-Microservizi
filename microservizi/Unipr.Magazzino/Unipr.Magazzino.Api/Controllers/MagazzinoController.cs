using Microsoft.AspNetCore.Mvc;
using Magazzino.Business.Abstraction;
using Magazzino.Shared;

namespace Magazzino.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class MagazzinoController(IBusiness business) : ControllerBase
{
    [HttpPost(Name = "Rifornisci")]
    public async Task<ActionResult> Rifornisci(ArticoloInsertDto dto)
    {
        await business.RifornisciMagazzinoAsync(dto);
        return Ok("Articoli aggiunti al magazzino con successo!");
    }

    [HttpGet("Verifica")]
    public async Task<ActionResult<bool>> VerificaDisponibilita([FromQuery] string codiceArticolo, [FromQuery] int quantita)
    {
        var disponibile = await business.VerificaDisponibilitaAsync(codiceArticolo, quantita);
        return Ok(disponibile);
    }
}