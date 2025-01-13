using System.ComponentModel.DataAnnotations;

namespace RESTAPI.Models;

public class Usuario
{
    [Key]
    public int Id { get; set; }

    public string nombreUsuario { get; set; }
    public string Nombre { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }
}