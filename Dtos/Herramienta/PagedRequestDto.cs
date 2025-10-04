using System.ComponentModel.DataAnnotations;

namespace pyreApi.DTOs.Herramienta
{
    public class PagedRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "La página debe ser mayor a 0")]
        public int Page { get; set; } = 1;

        [Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100")]
        public int PageSize { get; set; } = 10;

        public string? Search { get; set; }
    }
}
