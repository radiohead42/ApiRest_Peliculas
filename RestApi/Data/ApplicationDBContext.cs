using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using RESTAPI.Models;

namespace RESTAPI.Data
{
    public class ApplicationDBContext : IdentityDbContext<AppUsuario>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

        // Otros DbSets de tus entidades
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Pelicula> Peliculas { get; set; }
        
        // Solo se necesita DbSet<AppUsuario>
        public DbSet<AppUsuario> AppUsuarios { get; set; }
    }
}