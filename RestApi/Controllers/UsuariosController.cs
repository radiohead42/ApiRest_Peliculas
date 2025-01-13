using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using RESTAPI.Models;
using RestApi.Models.Dtos;
using RESTAPI.Respositorio.IRepositorio;

namespace RestApi.Controllers;

[Route("api/usuarios")]
[ApiController]
public class UsuarioController : ControllerBase
{
    private readonly IUsuarioRepositorio _usRepo;
    private readonly IMapper _mapper;
    protected RespuestasApi _respuestasApi;

    public UsuarioController(IUsuarioRepositorio usRepo, IMapper mapper)
    {
        _usRepo = usRepo;
        _mapper = mapper;
        this._respuestasApi = new();
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetUsuarios()
    {
        var listaUsuarios = _usRepo.GetUsuarios();
        var listaUsuariosDto = new List<UsuarioDto>();

        foreach (var lista in listaUsuarios)
        {
            listaUsuariosDto.Add(_mapper.Map<UsuarioDto>(lista));
        }

        return Ok(listaUsuariosDto);
    }
    
    [Authorize(Roles = "Admin")]
    [HttpGet("{usuarioId}", Name = "GetUsuario")]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetUsuario(string usuarioId)
    {
        var itemUsuario = _usRepo.GetUsuario(usuarioId);

        if (itemUsuario == null)
        {
            return NotFound();
        }

        var itemUsuarioDto = _mapper.Map<UsuarioDto>(itemUsuario);
        return Ok(itemUsuarioDto);
    }
    
    [AllowAnonymous]
    [HttpPost("registro")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Registro([FromBody] UsuarioRegistroDto usuarioRegistroDto)
    {
        // Verificar si el nombre de usuario ya existe
        bool validarNombreUsuarioUnicio = _usRepo.IsUniqueUser(usuarioRegistroDto.nombreUsuario);

        if (!validarNombreUsuarioUnicio)
        {
            _respuestasApi.StatusCode = HttpStatusCode.BadRequest;
            _respuestasApi.IsSucces = false;
            _respuestasApi.ErrorMessage.Add("El nombre de usuario ya existe");
            return BadRequest(_respuestasApi);
        }

        // Registrar el usuario
        var usuario = await _usRepo.Registro(usuarioRegistroDto);

        if (usuario == null)
        {
            _respuestasApi.StatusCode = HttpStatusCode.BadRequest;
            _respuestasApi.IsSucces = false;
            _respuestasApi.ErrorMessage.Add("Error en el registro");
            return BadRequest(_respuestasApi);
        }

        // Asignar el usuario al campo 'Result' en la respuesta
        _respuestasApi.StatusCode = HttpStatusCode.OK;
        _respuestasApi.IsSucces = true;
        _respuestasApi.Result = usuario;  // Asegúrate de asignar el usuario aquí

        // Devolver la respuesta con el usuario registrado
        return Ok(_respuestasApi);
    }


    
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] UsuarioLoginDto usuarioLoginDto)
    {
        var respuestaLogin = await _usRepo.Login(usuarioLoginDto);

        if (respuestaLogin.Usuario == null || string.IsNullOrEmpty(respuestaLogin.Token))
        {
            _respuestasApi.StatusCode = HttpStatusCode.BadRequest;
            _respuestasApi.IsSucces = false;
            _respuestasApi.ErrorMessage.Add("El nombre de usuario o password son incorrectos");
            return BadRequest(_respuestasApi);
        }
        
        _respuestasApi.StatusCode = HttpStatusCode.OK;
        _respuestasApi.IsSucces = true;
        _respuestasApi.Result = respuestaLogin;
        return Ok(_respuestasApi);

    }
}