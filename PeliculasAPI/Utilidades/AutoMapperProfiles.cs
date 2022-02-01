using AutoMapper;
using NetTopologySuite.Geometries;
using PeliculasAPI.DTO;
using PeliculasAPI.Entidades;
using System.Collections.Generic;

namespace PeliculasAPI.Utilidades
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles(GeometryFactory geometryFactory)
        {
            CreateMap<Genero, GeneroDTO>().ReverseMap();
            CreateMap<GeneroCreacionDTO, Genero>();

            CreateMap<Actor, ActorDTO>().ReverseMap();
            CreateMap<ActorCreacionDTO, Actor>()
                .ForMember(x => x.Foto, options => options.Ignore());

            CreateMap<CineCreacionDTO, Cine>()
                .ForMember(x => x.Ubicacion, x => x.MapFrom(dto => geometryFactory.CreatePoint(new Coordinate(dto.Latitud, dto.Longitud))));
            CreateMap<Cine, CineDTO>()
                .ForMember(x => x.Latitud, dto => dto.MapFrom(campo => campo.Ubicacion.Y))
                .ForMember(x => x.Longitud, dto => dto.MapFrom(campo => campo.Ubicacion.X));

            CreateMap<PeliculaCreacionDTO, Pelicula>()
                .ForMember(x => x.Poster, opciones => opciones.Ignore())
                .ForMember(x => x.GenerosPelicula, opciones => opciones.MapFrom(MapearPeliculasGeneros))
                .ForMember(x => x.PeliculasCines, opciones => opciones.MapFrom(MapearPeliculasCines))
                .ForMember(x => x.PeliculasActores, opciones => opciones.MapFrom(MapearPeliculasActores));

            CreateMap<Pelicula, PeliculaDTO>()
                .ForMember( x => x.Generos , opciones => opciones.MapFrom(MapearPeliculasGeneros))
                .ForMember( x => x.Actores , opciones => opciones.MapFrom(MapearPeliculasActores))
                .ForMember( x => x.Cines , opciones => opciones.MapFrom(MapearPeliculasCine));
        }

        private List<CineDTO> MapearPeliculasCine ( Pelicula pelicula, PeliculaDTO peliculaDTO)
        {
            var result = new List<CineDTO> ();  
            if (pelicula.PeliculasCines != null)
            {
                foreach( var cine in pelicula.PeliculasCines)
                {
                    result.Add(new CineDTO()
                    {
                        Id = cine.CineId,
                        Nombre = cine.Cine.Nombre,
                        Latitud = cine.Cine.Ubicacion.X,
                        Longitud = cine.Cine.Ubicacion.Y
                    });
                }
            }
            return result;
        }

        private List<GeneroDTO> MapearPeliculasGeneros( Pelicula pelicula, PeliculaDTO peliculaDTO)
        {
            var result = new List<GeneroDTO>();
            if (pelicula.GenerosPelicula != null)
            {
                foreach( var genero in pelicula.GenerosPelicula)
                {
                    result.Add(new GeneroDTO()
                    {
                        Id = genero.GeneroId,
                        Nombre = genero.Genero.Nombre
                    }); ;
                }
            }
            return result;
        }

        private List<PeliculaActorDTO> MapearPeliculasActores (Pelicula pelicula, PeliculaDTO peliculaDTO)
        {
            var result = new List<PeliculaActorDTO>();
            if (pelicula.PeliculasActores != null)
            {
                foreach(var actor in pelicula.PeliculasActores)
                {
                    result.Add(new PeliculaActorDTO()
                    {
                        Id = actor.ActorId,
                        Nombre = actor.Actor.Nombre,
                        Foto = actor.Actor.Foto,  
                        Orden = actor.Orden,
                        Personaje = actor.Personaje
                    });
                }
            }
            return result;
        }

        private List<GenerosPelicula> MapearPeliculasGeneros( PeliculaCreacionDTO peliculaCreacionDTO , Pelicula pelicula)
        {
            var resultado = new List<GenerosPelicula>();
            if (peliculaCreacionDTO.GenerosIds == null)
            {
                return resultado;
            }
            foreach (var id in peliculaCreacionDTO.GenerosIds)
            {
                resultado.Add(new GenerosPelicula()
                {
                    GeneroId = id
                });
            }
            return resultado;
        }

        private List<PeliculasCines> MapearPeliculasCines(PeliculaCreacionDTO peliculaCreacionDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculasCines>();
            if (peliculaCreacionDTO.CinesIds == null)
            {
                return resultado;
            }
            foreach (var id in peliculaCreacionDTO.CinesIds)
            {
                resultado.Add(new PeliculasCines()
                {
                    CineId = id
                });
            }
            return resultado;
        }

        private List<PeliculasActores> MapearPeliculasActores(PeliculaCreacionDTO peliculaCreacionDTO, Pelicula pelicula)
        {
            var resultado = new List<PeliculasActores>();

            if (peliculaCreacionDTO.Actores == null)
            {
                return resultado;
            }
            foreach (var actor in peliculaCreacionDTO.Actores)
            {
                resultado.Add(new PeliculasActores()
                {
                    ActorId = actor.Id ,
                    Personaje = actor.Personaje
                });
            }
            return resultado;
        }


    }
}
