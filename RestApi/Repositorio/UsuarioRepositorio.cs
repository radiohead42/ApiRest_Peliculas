using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Elfie.Serialization;
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
        throw new NotImplementedException();
    }

    // public AppUsuario GetUsuario(string usuarioId)
    // {
    //     return _bd.AppUsuarios.FirstOrDefault( c => c.Id == usuarioId);
    // }

    ICollection<AppUsuario> IUsuarioRepositorio.GetUsuarios()
    {
        throw new NotImplementedException();
    }

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
        // var passwordEncriptado = obtenermd5(usuarioLoginDto.Password);
        var usuario = _bd.AppUsuarios.FirstOrDefault(
            u => u.UserName.ToLower() == usuarioLoginDto.nombreUsuario.ToLower());

        bool isValid = await _userManager.CheckPasswordAsync(usuario, usuarioLoginDto.Password);

        if (usuario == null || isValid == false)
        {
            return new UsuarioLoginRespuestaDto()
            {
                Token = "",
                Usuario = null
            };
        }

        var roles = await _userManager.GetRolesAsync(usuario);
        
        var manejadorToken = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(claveSecreta);
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, usuario.UserName.ToString()),
                new Claim(ClaimTypes.Role, roles.FirstOrDefault())
                // Asegúrate de que 'usuario.Role' no es nulo
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = manejadorToken.CreateToken(tokenDescriptor);

        UsuarioLoginRespuestaDto usuarioLoginRespuestaDto = new UsuarioLoginRespuestaDto()
        {
            Token = manejadorToken.WriteToken(token),
            Usuario = _mapper.Map<UsuarioDatosDto>(usuario)
        };

        return usuarioLoginRespuestaDto;
    }

    public async Task<UsuarioDatosDto> Registro(UsuarioRegistroDto usuarioRegistroDto)
    {
        // var passwordEncriptado = obtenermd5(usuarioRegistroDto.Password);
        
        AppUsuario usuario = new AppUsuario()
        {
            UserName = usuarioRegistroDto.nombreUsuario,
            Email = usuarioRegistroDto.nombreUsuario,
            NormalizedEmail = usuarioRegistroDto.nombreUsuario.ToUpper(),
            Nombre = usuarioRegistroDto.Nombre,  // Asegúrate de que 'Role' no sea null
        };

        var result = await _userManager.CreateAsync(usuario, usuarioRegistroDto.Password);

        if (result.Succeeded)
        {
            if (!_roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
                await _roleManager.CreateAsync(new IdentityRole("Registrado"));
            }

            await _userManager.AddToRoleAsync(usuario, "Admin");
            var usuarioRetornado = _bd.AppUsuarios.FirstOrDefault( u => u.UserName == usuarioRegistroDto.nombreUsuario );
            
            return _mapper.Map<UsuarioDatosDto>(usuarioRetornado);
            
        }

        // _bd.Usuarios.Add(usuario);
        // await _bd.SaveChangesAsync();
        //
        // // Revisa que el usuario no sea null
        // if (usuario == null)
        // {
        //     throw new Exception("Error al guardar el usuario en la base de datos.");
        // }
        //
        // usuario.Password = passwordEncriptado;
        return new UsuarioDatosDto();
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