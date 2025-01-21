using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RESTAPI.Data;
using RESTAPI.Models;
using RestApi.Models.Dtos;

namespace RESTAPI.Respositorio.IRepositorio;

public class UsuarioRepositorio : IUsuarioRepositorio
{
    private readonly ApplicationDBContext _bd;
    private string claveSecreta;
    private readonly UserManager<AppUsuario> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMapper _mapper;

    public UsuarioRepositorio
        (ApplicationDBContext bd, 
            IConfiguration config, 
            UserManager<AppUsuario> userManager, 
            RoleManager<IdentityRole> roleManager,
            IMapper mapper)
    {
        _bd = bd;
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
        claveSecreta = config.GetValue<string>("ApiSettings:Secreta");
    }

    public ICollection<AppUsuario> GetUsuarios()
    {
        return _bd.AppUsuarios.OrderBy(c => c.UserName).ToList();
    }

    public AppUsuario GetUsuario(string usuarioId)
    {
        return _bd.AppUsuarios.FirstOrDefault(c => c.Id == usuarioId);
    }

    // public AppUsuario GetUsuario(string usuarioId)
    // {
    //     return _bd.AppUsuarios.FirstOrDefault( c => c.Id == usuarioId);
    // }

    // ICollection<AppUsuario> IUsuarioRepositorio.GetUsuarios()
    // {
    //     throw new NotImplementedException();
    // }

    public bool IsUniqueUser(string usuario)
    {
        var usuarioBd = _bd.AppUsuarios.FirstOrDefault(c => c.UserName == usuario);
        if (usuarioBd == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public async Task<UsuarioLoginRespuestaDto> Login(UsuarioLoginDto usuarioLoginDto)
{
    var usuario = _bd.AppUsuarios.FirstOrDefault(
        u => u.UserName.ToLower() == usuarioLoginDto.nombreUsuario.ToLower());

    // Si el usuario no existe, retornamos un resultado vacío
    if (usuario == null)
    {
        return new UsuarioLoginRespuestaDto()
        {
            Token = "",
            Usuario = null
        };
    }

    // Verificar si la contraseña es correcta
    bool isValid = await _userManager.CheckPasswordAsync(usuario, usuarioLoginDto.Password);

    if (!isValid)
    {
        return new UsuarioLoginRespuestaDto()
        {
            Token = "",
            Usuario = null
        };
    }

    // Obtener roles del usuario
    var roles = await _userManager.GetRolesAsync(usuario);
    
    // Asegurarnos de que roles no sea vacío
    if (roles == null || !roles.Any())
    {
        throw new Exception("El usuario no tiene roles asignados.");
    }

    // Crear lista de claims
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuario.UserName)
    };

    // Agregar todos los roles del usuario
    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    // Generación del token
    var manejadorToken = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(claveSecreta);

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(claims),
        Expires = DateTime.UtcNow.AddDays(7),
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(key),
            SecurityAlgorithms.HmacSha256Signature)
    };

    var token = manejadorToken.CreateToken(tokenDescriptor);

    // Preparar la respuesta
    UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
    {
        Token = manejadorToken.WriteToken(token),
        Usuario = _mapper.Map<UsuarioDatosDto>(usuario)
    };

    return usuarioLoginRespuestaDto;
}



    public async Task<UsuarioDatosDto> Registro(UsuarioRegistroDto usuarioRegistroDto)
    {
        AppUsuario usuario = new AppUsuario()
        {
            UserName = usuarioRegistroDto.nombreUsuario,
            Email = usuarioRegistroDto.nombreUsuario,
            NormalizedEmail = usuarioRegistroDto.nombreUsuario.ToUpper(),
            Nombre = usuarioRegistroDto.Nombre,
        };

        var result = await _userManager.CreateAsync(usuario, usuarioRegistroDto.Password);

        if (result.Succeeded)
        {
            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("Registrado"));
            }

            await _userManager.AddToRoleAsync(usuario, "Registrado"); // Cambia "Admin" a "Registrado" si es necesario.

            var usuarioRetornado = await _bd.AppUsuarios.FirstOrDefaultAsync(u => u.UserName == usuarioRegistroDto.nombreUsuario);

            if (usuarioRetornado == null)
            {
                throw new Exception("El usuario no fue encontrado después del registro.");
            }

            return _mapper.Map<UsuarioDatosDto>(usuarioRetornado);
        }

        var errorMessages = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new Exception($"Error al crear el usuario: {errorMessages}");
    }

    //
    //
    // public static string obtenermd5(string valor)
    // {
    //     MD5CryptoServiceProvider x = new MD5CryptoServiceProvider();
    //     byte[] data = System.Text.Encoding.UTF8.GetBytes(valor);
    //     data = x.ComputeHash(data);
    //     string resp = "";
    //     for (int i = 0; i < data.Length; i++)
    //     {
    //         resp += data[i].ToString("x2").ToLower();
    //     }
    //     return resp;
    // }
}