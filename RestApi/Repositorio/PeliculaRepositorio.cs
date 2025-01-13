using Microsoft.EntityFrameworkCore;
using RESTAPI.Data;
using RESTAPI.Models;
using RESTAPI.Respositorio.IRepositorio;

namespace RESTAPI.Repositorio
{
    public class PeliculaRepositorio: IPeliculaRepositorio
    {
        private readonly ApplicationDBContext _bd;
        public PeliculaRepositorio(ApplicationDBContext bd)
        {
            _bd = bd;
        }

        public bool ActualizarPelicula(Pelicula pelicula)
        {
            pelicula.FechaCreacion = DateTime.Now;
            var peliculaExistente = _bd.Peliculas.Find(pelicula.Id);
            if(peliculaExistente != null)
            {
                _bd.Entry(peliculaExistente).CurrentValues.SetValues(pelicula);
            }
            else
            {
                _bd.Peliculas.Update(pelicula);
            }
            
            return Guardar();
        }

        public bool BorrarPelicula(Pelicula pelicula)
        {
            _bd.Peliculas.Remove(pelicula);
            return Guardar();
        }

        public bool CrearPelicula(Pelicula pelicula)
        {
            pelicula.FechaCreacion= DateTime.Now;
            _bd.Peliculas.Add(pelicula);
            return Guardar();  
        }

        public bool ExistePelicula(int id)
        {
            return _bd.Peliculas.Any(c => c.Id == id);
        }

        public bool ExistePelicula(string nombre)
        {
            bool valor = _bd.Peliculas.Any(c => c.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
            return valor;
        }

        public IEnumerable<Pelicula> BuscarPelicula(string nombre)
        {
            IQueryable<Pelicula> query = _bd.Peliculas;
            if (!string.IsNullOrEmpty(nombre))
            {
                query = query.Where(e => e.Nombre.Contains(nombre) || e.Descripcion.Contains(nombre));
            }

            return query.ToList();
        }

        public Pelicula GetPelicula(int PeliculaId)
        {
            return _bd.Peliculas.FirstOrDefault(c => c.Id == PeliculaId);
        }

        public ICollection<Pelicula> GetPeliculas()
        {
            return _bd.Peliculas.OrderBy(c => c.Nombre).ToList();
        }

        public ICollection<Pelicula> GetPeliculasEnCategoria(int catId)
        {
            return _bd.Peliculas.Include(ca => ca.Categoria).Where(ca => ca.categoriaId == catId).ToList();
        }

        public bool Guardar()
        {
            return _bd.SaveChanges() > 0;
        }
    }
}