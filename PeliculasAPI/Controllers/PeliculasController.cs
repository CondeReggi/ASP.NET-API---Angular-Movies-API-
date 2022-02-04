using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    [Route("api/peliculas")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class PeliculasController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IAlmacenadorArchivos almacenadorArchivos;
        private readonly UserManager<IdentityUser> userManager;
        private readonly string contenedor = "peliculas";

        public PeliculasController(ApplicationDbContext context, IMapper mapper, IAlmacenadorArchivos almacenadorArchivos, UserManager<IdentityUser> userManager)
        {
            this.context = context;
            this.mapper = mapper;
            this.almacenadorArchivos = almacenadorArchivos;
            this.userManager = userManager;
        }

        [HttpPost]
        public async Task<ActionResult<int>> Post([FromForm] PeliculaCreacionDTO peliculaCreacionDTO)
        {
            var pelicula = mapper.Map<Pelicula>(peliculaCreacionDTO);

            if (peliculaCreacionDTO.Poster != null)
            {
                pelicula.Poster = await almacenadorArchivos.GuardarArchivo(contenedor, peliculaCreacionDTO.Poster);
            }

            EscribirOrdenActores(pelicula);
            context.Add(pelicula);

            await context.SaveChangesAsync();
            return pelicula.Id;
        }

        [HttpGet("PostGet")]
        public async Task<ActionResult<PeliculasPostGetDTO>> PostGet()
        {
            var cines = await context.Cines.ToListAsync();
            var generos = await context.Generos.ToListAsync();

            var cinesDTO = mapper.Map<List<CineDTO>>(cines);    
            var generosDTO = mapper.Map<List<GeneroDTO>>(generos);

            return new PeliculasPostGetDTO()
            {
                Cines = cinesDTO,
                Generos = generosDTO
            };
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<LandingPageDTO>> LandingPageGet()
        {
            var resultado = new LandingPageDTO();

            var top = 6;
            var listadoEnCines = await context.Peliculas
                                    .Where( x => x.EnCines )
                                    .Take(top)
                                    .OrderBy(x => x.Titulo)
                                    .ToListAsync();

            var listadoProximosEstrenos = await context.Peliculas
                                                .Where(x => x.FechLanzamiento > DateTime.Now)
                                                .Take(top)
                                                .OrderBy(x => x.Titulo)
                                                .ToListAsync();

            var listadoPeliculasViejas = await context.Peliculas
                                                .Where(x => x.FechLanzamiento < DateTime.Now)
                                                .Take(top)
                                                .OrderBy(x => x.Titulo)
                                                .ToListAsync();

            resultado.ProximosEstrenos = mapper.Map<List<PeliculaDTO>>(listadoProximosEstrenos);
            resultado.EnCines = mapper.Map<List<PeliculaDTO>>(listadoEnCines);
            resultado.Anteriores = mapper.Map<List<PeliculaDTO>>(listadoPeliculasViejas);

            return resultado;
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PeliculaDTO>> Get(int id)
        {
            var pelicula = await context.Peliculas
                                    .Include( x => x.GenerosPelicula ).ThenInclude( x => x.Genero )
                                    .Include(x => x.PeliculasActores).ThenInclude(x => x.Actor)
                                    .Include(x => x.PeliculasCines).ThenInclude(x => x.Cine)
                                    .FirstOrDefaultAsync( x => x.Id == id );

            if (pelicula == null)
            {
                return NotFound();
            }

            var promedioVoto = 0.0;
            var usuarioVoto = 0;



            if ( await context.Ratings.AnyAsync(x => x.PeliculaId == id))
            {

                promedioVoto = await context.Ratings.Where(x => x.PeliculaId == id).AverageAsync(x => x.Puntuacion);

                //Obtener email del usuario
                var email = HttpContext.User.Claims.FirstOrDefault(x => x.Type == "email").Value;
                var usuario = await userManager.FindByEmailAsync(email);
                var usuarioId = usuario.Id; //Obtengo la id
                var votoUsuario = await context.Ratings.FirstOrDefaultAsync(x => x.UsuarioId == usuarioId && x.PeliculaId == id);


                if (votoUsuario != null)
                {
                    usuarioVoto = votoUsuario.Puntuacion;
                }
            }

            var dto = mapper.Map<PeliculaDTO>(pelicula);
            dto.VotoUsuario = usuarioVoto;
            dto.PromedioVotos = promedioVoto;

            return dto;
        }

        [HttpGet("GetPut/{id:int}")]
        [AllowAnonymous]
        public async Task<ActionResult<PeliculasPutGetDTO>> PutGet(int id)
        {
            var peliculaActionResult = await Get(id);
            
            if (peliculaActionResult.Result is NotFoundResult)
            {
                return NotFound();
            }

            var pelicula = peliculaActionResult.Value;
            var generosSeleccionados = pelicula.Generos.Select(x => x.Id).ToList(); // Tomo todos los generos y los convierto en una lista
            var generosNoSeleccionados = await context.Generos.Where(x => !generosSeleccionados.Contains(x.Id)).ToListAsync();

            var cinesSeleccionados = pelicula.Cines.Select(x => x.Id).ToList(); // Tomo todos los generos y los convierto en una lista
            var cinesNoSeleccionados = await context.Cines.Where(x => !cinesSeleccionados.Contains(x.Id)).ToListAsync();

            var generosNoSeleccionadosDTO = mapper.Map<List<GeneroDTO>>(generosNoSeleccionados);
            var cinesNoSeleccionadosDTO = mapper.Map<List<CineDTO>>(cinesNoSeleccionados);

            var result = new PeliculasPutGetDTO();

            result.Pelicula = pelicula;
            result.GenerosSeleccionados = pelicula.Generos;
            result.CinesSeleccionados = pelicula.Cines;
            result.GenerosNoSeleccionados = generosNoSeleccionadosDTO;
            result.CinesNoSeleccionados = cinesNoSeleccionadosDTO;
            result.Actores = pelicula.Actores;

            return result;
        }

        [HttpGet("filtrar")]
        [AllowAnonymous]
        public async Task<ActionResult<List<PeliculaDTO>>> GetFiltro([FromQuery] PeliculasFiltrarDTO peliculaFiltroDTO)
        {
            var peliculasQueryAble = context.Peliculas.AsQueryable();

            if (!string.IsNullOrEmpty(peliculaFiltroDTO.Titulo))
            {
                peliculasQueryAble = peliculasQueryAble.Where(x => x.Titulo.Contains(peliculaFiltroDTO.Titulo));
            }

            if (peliculaFiltroDTO.EnCines)
            {
                peliculasQueryAble = peliculasQueryAble.Where(x => x.EnCines);
            }

            if (peliculaFiltroDTO.ProximosEstrenos)
            {
                peliculasQueryAble = peliculasQueryAble.Where(x => x.FechLanzamiento > DateTime.Now);
            }

            if (peliculaFiltroDTO.GeneroId != 0 )
            {
                peliculasQueryAble = peliculasQueryAble.Where(x => x.GenerosPelicula.Select(x => x.GeneroId).Contains(peliculaFiltroDTO.GeneroId) );
            }

            await HttpContext.InsertarParametrosPaginacionEnCabecera(peliculasQueryAble);

            var peliculas = await peliculasQueryAble.Paginar(peliculaFiltroDTO.paginacionDTO).ToListAsync();

            return mapper.Map<List<PeliculaDTO>>(peliculas);
        }


        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existe = await context.Peliculas.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            context.Remove(new Pelicula() { Id = id });
            await context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, [FromForm] PeliculaCreacionDTO peliculaCreacionDTO)
        {
            //var existeRegistro = await context.Peliculas.AnyAsync(x => x.Id == id);

            //if (!existeRegistro)
            //{
            //    return NotFound();
            //}

            //var pelicula = await context.Peliculas.FirstOrDefaultAsync(x => x.Id == id);
            //pelicula = mapper.Map<Pelicula>(peliculaCreacionDTO);

            //await context.SaveChangesAsync();
            //return NoContent();

            var peliculaAModificar = await context.Peliculas
                                                .Include(x => x.PeliculasActores)
                                                .Include(x => x.PeliculasCines)
                                                .Include(x => x.GenerosPelicula)
                                                .FirstOrDefaultAsync(x => x.Id == id);

            if ( peliculaAModificar == null)
            {
                return NotFound();
            }

            peliculaAModificar = mapper.Map(peliculaCreacionDTO, peliculaAModificar);

            if (peliculaCreacionDTO.Poster != null)
            {
                peliculaAModificar.Poster = await almacenadorArchivos.EditarArchivo(contenedor, peliculaCreacionDTO.Poster, peliculaAModificar.Poster);
            }

            EscribirOrdenActores(peliculaAModificar);

            await context.SaveChangesAsync();
            return NoContent();
        }

        private void EscribirOrdenActores( Pelicula pelicula)
        {
            if (pelicula.PeliculasActores != null)
            {
                for (int i = 0; i < pelicula.PeliculasActores.Count; i++)
                {
                    pelicula.PeliculasActores[i].Orden = i; //Se supone que por cada actore le estoy poniendo un "id" para posteriormente ordenarlos en orden
                }
            }
        }

    }
}
