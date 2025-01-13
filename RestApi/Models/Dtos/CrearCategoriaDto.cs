using System.ComponentModel.DataAnnotations;

namespace RESTAPI.Models.CrearCategoriaDto
{
    public class CrearCategoriaDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(100, ErrorMessage = "El numero maximo de caracteres es de cien")]
        public string Nombre { get; set; }
    }
}