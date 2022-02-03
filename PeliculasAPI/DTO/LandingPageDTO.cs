using System.Collections.Generic;

namespace PeliculasAPI.DTO
{
    public class LandingPageDTO
    {
        public List<PeliculaDTO> EnCines { get; set; }
        public List<PeliculaDTO> ProximosEstrenos { get; set; }

        public List<PeliculaDTO> Anteriores { get; set; }
    }
}
