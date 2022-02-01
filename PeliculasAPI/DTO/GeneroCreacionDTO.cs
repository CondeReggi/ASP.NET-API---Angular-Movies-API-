using PeliculasAPI.Validaciones;
using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.DTO
{
    public class GeneroCreacionDTO
    {

        [Required]
        [StringLength(100)]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
    }
}
