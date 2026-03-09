using Backend.Data;
using Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers;

[ApiController]
[Authorize]
[Route("api/sexos")]
public class SexosController : ControllerBase
{
    private readonly AppDbContext _db;

    public SexosController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var sexos = await _db.Sexos
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Select(s => new SexoResponse
            {
                Id = s.Id,
                Nombre = s.Nombre
            })
            .ToListAsync();

        return Ok(sexos);
    }
}