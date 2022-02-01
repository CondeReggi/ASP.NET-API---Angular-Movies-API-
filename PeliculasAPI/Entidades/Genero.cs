using PeliculasAPI.Validaciones;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PeliculasAPI.Entidades
{
    public class Genero
    {

        public int Id { get; set; }

        [Required]
        [StringLength(100)] 
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
        public List<GenerosPelicula> GenerosPelicula { get; set; }


        // Se pueden tener validaciones aca tambien, pero primero cumplen todas las de  forma [] y despues evalua las declaradas aca
    }
}
