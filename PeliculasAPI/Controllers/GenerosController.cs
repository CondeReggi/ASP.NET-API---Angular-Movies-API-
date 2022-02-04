using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PeliculasAPI.DTO;
using PeliculasAPI.Entidades;
using PeliculasAPI.Utilidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class GenerosController : ControllerBase
    {
        private readonly ILogger<GenerosController> logger;
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public GenerosController(ILogger<GenerosController> logger, ApplicationDbContext context, IMapper mapper)
        {
            this.logger = logger;
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<GeneroDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Generos.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable);

            var generos = await queryable.OrderBy(x => x.Nombre).Paginar(paginacionDTO).ToListAsync();
            var generosDTO = mapper.Map<List<GeneroDTO>>(generos); // <--- Importante si retornas un listado y haces un mapeo poner todo <List<Coso>>

            if (generosDTO == null)
            {
                return NotFound();
            }

            return generosDTO;
        }

        [HttpGet("todos")]
        [AllowAnonymous]
        public async Task<ActionResult<List<GeneroDTO>>> GetTodos()
        {
            var generos = await context.Generos.ToListAsync();

            if (generos == null)
            {
                return NotFound();
            }

            return mapper.Map<List<GeneroDTO>>(generos);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<GeneroDTO>> GetGenero(int id)
        {
            var genero = await context.Generos.FirstOrDefaultAsync(x => x.Id == id);

            if (genero == null)
            {
                return NotFound();
            }

            return mapper.Map<GeneroDTO>(genero);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GeneroCreacionDTO genero)
        {
            var generoCreacion = mapper.Map<Genero>(genero);

            context.Add(generoCreacion);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] GeneroCreacionDTO generoCreacionDTO)
        {
            var existeRegistro = await context.Generos.AnyAsync(x => x.Id == id);

            if (!existeRegistro)
            {
                return NotFound();
            }

            var genero = await context.Generos.FirstOrDefaultAsync(x => x.Id == id);
            genero = mapper.Map(generoCreacionDTO, genero);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var generoBorrar = await context.Generos.AnyAsync(x => x.Id==id);

            if (!generoBorrar)
            {
                return NotFound();
            }

            context.Remove( new Genero() { Id = id } );
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
