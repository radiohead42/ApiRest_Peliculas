using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using RESTAPI.Models;
using RESTAPI.Models.CrearCategoriaDto;
using RESTAPI.Models.Dto.CategoriaDto;
using RESTAPI.Respositorio.IRepositorio;

//[ResponseCache(Duration = 20)]
[Route("api/categorias")]
//[Authorize Roles = "Admin"]
// [ApiVersion("1.0")]
// [ApiVersion("2.0")]
[ApiController]
[EnableCors("politicaCors")]//Definir de forma global
public class CategoriasController : ControllerBase
{
    
    private readonly ICategoriaRepositorio _ctRepo;
    private readonly IMapper _mapper;

    public CategoriasController(ICategoriaRepositorio ctRepo, IMapper mapper)
    {
        _ctRepo = ctRepo;
        _mapper = mapper;
    }
    
    [AllowAnonymous]
    [HttpGet]
    [MapToApiVersion("1.0")]
    [ResponseCache(CacheProfileName = "Defecto20Seg")]//Tiempo en segundos
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    //[EnableCors("PoliticaCors")]//Aplica la politica CORS a un solo metodo
    public IActionResult GetCategorias()
    {
        var listaCategorias = _ctRepo.GetCategorias();
        var listasCategoriasDto = new List<CategoriaDto>();
        foreach (var item in listaCategorias)
        {
            listasCategoriasDto.Add(_mapper.Map<CategoriaDto>(item));
        }
        return Ok(listasCategoriasDto);
    }

    [AllowAnonymous]
    [HttpGet("{categoriaId:int}",Name = "GetCategoria")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]     
    public IActionResult GetCategoria(int categoriaId)
    {
        var categoria = _ctRepo.GetCategoria(categoriaId);
        if(categoria == null)
        {
            return NotFound();
        }
        var itemCategoriaDto = _mapper.Map<CategoriaDto>(categoria);
        return Ok(itemCategoriaDto);
    }
    
    [AllowAnonymous]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CrearCategoria([FromBody] CrearCategoriaDto crearCategoriaDto)
    {
        // Verifica si el modelo es válido
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verifica si la categoría ya existe
        if (_ctRepo.ExisteCategoria(crearCategoriaDto.Nombre))
        {
            ModelState.AddModelError("", $"La categoria '{crearCategoriaDto.Nombre}' ya existe.");
            return BadRequest(ModelState); // Cambié a BadRequest en lugar de 404
        }

        // Mapea el DTO a la entidad
        var categoria = _mapper.Map<Categoria>(crearCategoriaDto);

        // Intenta guardar la categoría
        if (!_ctRepo.CrearCategoria(categoria))
        {
            ModelState.AddModelError("", "Algo salió mal guardando el registro");
            return StatusCode(500, ModelState); // Cambié a 500 para error interno
        }

        // Si todo fue exitoso, retorna el resultado con un 201 Created
        return CreatedAtRoute("GetCategoria", new { categoriaId = categoria.Id }, categoria);
    }

    
    [AllowAnonymous]
    [HttpPatch("{categoriaId:int}", Name = "ActualizarPatchCategoria")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult ActualizarPatchCategoria(int categoriaId, [FromBody] CategoriaDto categoriaDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if(categoriaDto == null || categoriaId != categoriaDto.Id)
        {
            return BadRequest(ModelState);
        }

        var categoria = _mapper.Map<Categoria>(categoriaDto);

        if(!_ctRepo.ActualizarCategoria(categoria))
        {
            ModelState.AddModelError("", $"Algo salio mal actualizando el registro");
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }
    [AllowAnonymous]
    [HttpPut("{categoriaId:int}", Name = "ActualizarPutCategoria")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ActualizarPutCategoria(int categoriaId, [FromBody] CategoriaDto categoriaDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if(categoriaDto == null || categoriaId != categoriaDto.Id)
        {
            return BadRequest(ModelState);
        }

        var categoriaExistente = _ctRepo.GetCategoria(categoriaId);

        if (categoriaExistente == null)
        {
            return NotFound($"No se encontro la categoria con ID {categoriaId}");
        }

        var categoria = _mapper.Map<Categoria>(categoriaDto);

        if(!_ctRepo.ActualizarCategoria(categoria))
        {
            ModelState.AddModelError("", $"Algo salio mal actualizando el registro");
            return StatusCode(500, ModelState);
        }

        return NoContent();
    }
    
    [AllowAnonymous]
    [HttpDelete("{categoriaId:int}", Name = "DeleteCategoria")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteCategoria(int categoriaId)
    {
        // Verifica si la categoría existe
        if (!_ctRepo.ExisteCategoria(categoriaId))
        {
            return NotFound();
        }

        var categoria = _ctRepo.GetCategoria(categoriaId);

        // Intenta borrar la categoría
        bool borradoExitoso = _ctRepo.BorrarCategoria(categoria);

        if (!borradoExitoso)
        {
            // Si algo salió mal, devuelve el error adecuado, pero sin agregar el ModelState
            return StatusCode(500, "Error al eliminar la categoría.");
        }

        // Si todo salió bien, devuelve una respuesta exitosa sin contenido (204)
        return NoContent();
    }

}