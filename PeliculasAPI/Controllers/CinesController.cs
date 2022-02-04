using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.DTO;
using PeliculasAPI.Entidades;
using PeliculasAPI.Utilidades;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasAPI.Controllers
{
    [ApiController]
    [Route("api/cines")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class CinesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public CinesController(ApplicationDbContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<CineDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = context.Cines.AsQueryable(); //Me hago un queryable con los Actroes
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable); //Inserto los params
            var cines = await queryable.OrderBy(x => x.Nombre).Paginar(paginacionDTO).ToListAsync();
            var cinesDTO = mapper.Map<List<CineDTO>>(cines); // <--- Importante si retornas un listado y haces un mapeo poner todo <List<Coso>>
            if (cinesDTO == null)
            {
                return NotFound();
            }
            return cinesDTO;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CineDTO>> GetById ( int id)
        {
            var cineRequerido = await context.Cines.FirstOrDefaultAsync(x => x.Id == id);

            if (cineRequerido == null)
            {
                return NotFound();
            }

            return mapper.Map<CineDTO>(cineRequerido);
        }


        [HttpPost]
        public async Task<ActionResult> Post([FromBody] CineCreacionDTO cineCreacionDTO)
        {
            var cineAgregar = mapper.Map<Cine>(cineCreacionDTO);
            context.Add(cineAgregar);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromBody] CineCreacionDTO cineCreacionDTO)
        {
            var cineSeleccionado = await context.Cines.FirstOrDefaultAsync(x => x.Id == id); //Agarro mi elemento

            if ( cineSeleccionado == null )
            {
                return NotFound(); //404 si no funciona
            }

            cineSeleccionado = mapper.Map(cineCreacionDTO , cineSeleccionado); // Lo mapeo con el nuevo creacionDTO, es decir sobrescribo los campos que cambiaron

            await context.SaveChangesAsync(); //guardo los cambios pa l abase de datos
            return NoContent();
        }

        [HttpDelete("{id:int}")] 
        public async Task<ActionResult> Delete(int id)
        {
            var existeCine = await context.Cines.AnyAsync(x => x.Id == id);
            
            if (!existeCine)
            {
                return NotFound();
            }

            context.Remove( new Cine() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
