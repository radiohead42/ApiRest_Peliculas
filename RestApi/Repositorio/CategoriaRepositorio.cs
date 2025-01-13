using RESTAPI.Data;
using RESTAPI.Models;
using RESTAPI.Respositorio.IRepositorio;

namespace RESTAPI.Repositorio
{
    public class CategoriaRepositorio: ICategoriaRepositorio
    {
        private readonly ApplicationDBContext _bd;
        public CategoriaRepositorio(ApplicationDBContext bd)
        {
            _bd = bd;
        }

        public bool ActualizarCategoria(Categoria categoria)
        {
            categoria.FechaCreacion = DateTime.Now;
            var categoriaExistente = _bd.Categorias.Find(categoria.Id);
            if(categoriaExistente != null)
            {
                _bd.Entry(categoriaExistente).CurrentValues.SetValues(categoria);
            }
            else
            {
                _bd.Categorias.Update(categoria);
            }
            
            return Guardar();
        }

        public bool BorrarCategoria(Categoria categoria)
        {
            try
            {
                _bd.Categorias.Remove(categoria);

                return Guardar();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar la categoría: {ex.Message}");
                return false;
            }
        }


        public bool CrearCategoria(Categoria categoria)
        {
            categoria.FechaCreacion= DateTime.Now;
            _bd.Categorias.Add(categoria);
            return Guardar();  
        }

        public bool ExisteCategoria(int id)
        {
            return _bd.Categorias.Any(c => c.Id == id);
        }

        public bool ExisteCategoria(string nombre)
        {
            bool valor = _bd.Categorias.Any(c => c.Nombre.ToLower().Trim() == nombre.ToLower().Trim());
            return valor;
        }

        public Categoria GetCategoria(int CategoriaId)
        {
            return _bd.Categorias.FirstOrDefault(c => c.Id == CategoriaId);
        }

        public ICollection<Categoria> GetCategorias()
        {
            return _bd.Categorias.OrderBy(c => c.Nombre).ToList();
        }

        public bool Guardar()
        {
            return _bd.SaveChanges() >= 0;
        }
    }
}