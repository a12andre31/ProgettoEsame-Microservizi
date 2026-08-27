using Microsoft.AspNetCore.Mvc;
using Ordini.Business.Abstraction;
using Ordini.Shared;

namespace Ordini.Api.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class OrdiniController(IBusiness business) : ControllerBase
{
    [HttpPost(Name = "CreateOrdine")]
    public async Task<ActionResult> CreateOrdine(OrdineInsertDto ordineInsertDto)
    {
        await business.CreateOrdineAsync(ordineInsertDto);
        return Ok("Ordine creato e inserito nella coda SAGA!");
    }
}