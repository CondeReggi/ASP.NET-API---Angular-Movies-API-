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
    [Route("api/actores")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class ActoresController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly string contenedor = "actores";
        //private PaginacionDTO paginacionDTO;

        public ActoresController(ApplicationDbContext context, IMapper mapper , IAlmacenadorArchivos almacenadorArchivos)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
        }

        [HttpGet]
        public async Task<ActionResult<List<ActorDTO>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            //var actores = await context.Actores.ToListAsync();
            //var actoresDTO = mapper.Map<List<ActorDTO>>(actores); // <--- Importante si retornas un listado y haces un mapeo poner todo <List<Coso>>

            //if (actoresDTO == null)
            //{
            //    return NotFound();
            //}

            //return actoresDTO;

            var queryable = context.Actores.AsQueryable(); //Me hago un queryable con los Actroes
            await HttpContext.InsertarParametrosPaginacionEnCabecera(queryable); //Inserto los params

            var actores = await queryable.OrderBy(x => x.Nombre).Paginar(paginacionDTO).ToListAsync();
            var actoresDTO = mapper.Map<List<ActorDTO>>(actores); // <--- Importante si retornas un listado y haces un mapeo poner todo <List<Coso>>

            if (actoresDTO == null)
            {
                return NotFound();
            }

            return actoresDTO;
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ActorDTO>> GetById(int id)
        {
            var actor = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

            if (actor == null)
            {
                return NotFound();
            }

            return mapper.Map<ActorDTO>(actor); 
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] ActorCreacionDTO actorCreacionDTO)
        {
            var actor = mapper.Map<Actor>(actorCreacionDTO);

            if (actorCreacionDTO.Foto != null)
            {
                actor.Foto = await almacenadorArchivos.GuardarArchivo(contenedor, actorCreacionDTO.Foto);
            }
 
            context.Add(actor);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] ActorCreacionDTO actorCreacionDTO)
        {
            var actor = await context.Actores.FirstOrDefaultAsync(x => x.Id==id);

            if (actor == null)
            {
                return NotFound();
            }

            actor = mapper.Map(actorCreacionDTO, actor);

            if (actorCreacionDTO.Foto != null)
            {
                actor.Foto = await almacenadorArchivos.EditarArchivo(contenedor, actorCreacionDTO.Foto, actor.Foto);
            }

            await context.SaveChangesAsync();
            return NoContent();

        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var actorBorrado = await context.Actores.FirstOrDefaultAsync(x => x.Id == id);

            if (actorBorrado == null)
            {
                return NotFound();
            }

            context.Remove(new Actor() { Id = id });
            await context.SaveChangesAsync();
            await almacenadorArchivos.BorrarArchivo(actorBorrado.Foto, contenedor); //Hay que borrarla de la nube

            return NoContent();
        }

        [HttpPost("buscarPorNombre")]
        public async Task<ActionResult<List<PeliculaActorDTO>>> BuscarPorNombre([FromBody] string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
            {
                return new List<PeliculaActorDTO>();
            }

            return await context.Actores
                            .Where(x => x.Nombre.Contains(nombre))
                            .Select(x => new PeliculaActorDTO
                            {
                                Id = x.Id,
                                Nombre = x.Nombre,
                                Foto = x.Foto
                            })
                            .Take(5)
                            .ToListAsync();

        }

    }
}
