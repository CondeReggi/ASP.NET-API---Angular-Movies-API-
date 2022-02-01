namespace PeliculasAPI.Entidades
{
    public class GenerosPelicula
    {
        public int PeliculaId { get; set; }
        public int GeneroId { get; set; }
        public Pelicula Pelicula { get; set; }
        public Genero Genero { get; set; }
    }
}
