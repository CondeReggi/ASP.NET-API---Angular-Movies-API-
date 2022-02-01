using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.Entidades
{
    public class Cine
    {
        public int Id { get; set; }
        [Required]
        [StringLength(105)]
        public string Nombre { get; set; }
        public Point Ubicacion { get; set; }
        public List<PeliculasCines> PeliculasCines { get; set; }

    }
}
