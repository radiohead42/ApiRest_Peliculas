using System.ComponentModel.DataAnnotations;

namespace RestApi.Models.Dtos;

public class UsuarioLoginDto
{
    [Required(ErrorMessage = "el usuario es obligatorio")]
    public string nombreUsuario { get; set; }
    
    [Required(ErrorMessage = "La contrasena es obligatorio")]
    public string Password { get; set; }
}