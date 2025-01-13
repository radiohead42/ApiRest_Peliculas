using System.ComponentModel.DataAnnotations;

namespace RESTAPI.Models.Dto.CategoriaDto
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El numero maximo de caracteres es de cien")]
        public string Nombre { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}