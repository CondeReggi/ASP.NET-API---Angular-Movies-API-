using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PeliculasAPI.Entidades;

namespace PeliculasAPI
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuider)
        {
            modelBuider.Entity<GenerosPelicula>().HasKey(x => new { x.PeliculaId, x.GeneroId });
            modelBuider.Entity<PeliculasActores>().HasKey(x => new { x.PeliculaId, x.ActorId });
            modelBuider.Entity<PeliculasCines>().HasKey(x => new { x.PeliculaId, x.CineId });

            base.OnModelCreating(modelBuider); // Es necesario cuando usas el Identity, es para conservar la identidad, y no perderla en caso de override
        }

        public DbSet<Genero> Generos { get; set; }
        public DbSet<Actor> Actores { get; set; }
        public DbSet<Cine> Cines { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        public DbSet<PeliculasActores> PeliculasActores { get; set; }
        public DbSet<PeliculasCines> PeliculasCines { get; set; }
        public DbSet<GenerosPelicula> GenerosPelicula { get; set; }
        public DbSet<Rating> Ratings { get; set; }


    }
}
