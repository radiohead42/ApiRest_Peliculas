using System.ComponentModel.DataAnnotations;

namespace RESTAPI.Models
{
    public class Categoria
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Nombre { get; set; }
        [Required]
        [Display(Name = "fecha de Creacion")]
        public DateTime FechaCreacion { get; set; }
    }
}