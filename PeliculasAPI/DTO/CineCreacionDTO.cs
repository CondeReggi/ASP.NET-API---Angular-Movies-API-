using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.DTO
{
    public class CineCreacionDTO
    {
        [Required]
        [StringLength(105)]
        public string Nombre { get; set; }
        [Range(-90,90)]
        public double Latitud { get; set; }
        [Range(-180,180)]
        public double Longitud { get; set; }   
    }
}
