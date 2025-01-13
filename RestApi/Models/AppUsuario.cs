using Microsoft.AspNetCore.Identity;

namespace RESTAPI.Models;

public class AppUsuario: IdentityUser
{
    public string Nombre { get; set; }
}