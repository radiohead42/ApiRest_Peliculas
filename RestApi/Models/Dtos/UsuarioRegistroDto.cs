using System.ComponentModel.DataAnnotations;

namespace RestApi.Models.Dtos;

public class UsuarioRegistroDto
{
    [Required(ErrorMessage = "el usuario es obligatorio")]
    public string nombreUsuario { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio")]
    public string Nombre { get; set; }
    [Required(ErrorMessage = "La contrasena es obligatorio")]
    public string Password { get; set; }

    public string Role { get; set; }
}