
using AutoMapper;
using RESTAPI.Models;
using RESTAPI.Models.CrearCategoriaDto;
using RESTAPI.Models.Dto.CategoriaDto;
using RestApi.Models.Dtos;

namespace RESTAPI.PeliculasMappers
{
    public class PeliculasMappers: Profile
    {
        public PeliculasMappers()
        {
            CreateMap<Categoria, CategoriaDto>().ReverseMap();
            CreateMap<Categoria, CrearCategoriaDto>().ReverseMap();
            
            CreateMap<Pelicula, PeliculaDto>().ReverseMap();
            CreateMap<Pelicula, CrearPeliculaDto>().ReverseMap();

            CreateMap<AppUsuario, UsuarioDatosDto>().ReverseMap();
            // CreateMap<Usuario, UsuarioLoginDto>().ReverseMap();
            // CreateMap<Usuario, UsuarioRegistroDto>().ReverseMap();
            // CreateMap<Usuario, UsuarioLoginRespuestaDto>().ReverseMap();
        }
    }
}