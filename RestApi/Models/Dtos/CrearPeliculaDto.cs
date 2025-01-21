namespace RestApi.Models.Dtos;

public class CrearPeliculaDto
{
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public int Duracion { get; set; }
    public string? RutaImagen { get; set; }
    public IFormFile Imagen { get; set; }
    public enum CrearTipoClasificacion { Siete, Trece, Dieciseis, Dieciocho}
    public CrearTipoClasificacion Clasificacion { get; set; }
    //Relacion con tabla cateforia
    public int categoriaId { get; set; }
}