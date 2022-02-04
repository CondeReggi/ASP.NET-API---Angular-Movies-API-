using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PeliculasAPI.DTO
{
    public class CredencialesUsuario
    {
        [EmailAddress]
        [Required]
        public string email { get; set; }
        [Required]
        public string password { get; set; }
    }
}
