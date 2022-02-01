using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.Entidades
{
    public class Pelicula
    {
        public int Id { get; set; }
        [Required]
        [StringLength(150)]
        public string Titulo { get; set; }
        public string Resumen { get; set; }
        public string Trailer { get; set; }
        public bool EnCines { get; set; }
        public DateTime FechLanzamiento { get; set; }
        public string Poster { get; set; }
        public List<GenerosPelicula> GenerosPelicula { get; set; }
        public List<PeliculasActores> PeliculasActores { get; set; }
        public List<PeliculasCines> PeliculasCines { get; set; }
    }
}
