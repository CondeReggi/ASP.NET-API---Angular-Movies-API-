namespace PeliculasAPI.DTO
{
    public class PaginacionDTO
    {
        public int Pagina { get; set; } = 1;
        private int reordsPorPagina = 10;
        private readonly int cantidadMaximaDeRegistros = 50;
            
        public int RecordsPorPagina {
            get
            {
                return reordsPorPagina;
            }
            set
            {
                reordsPorPagina = (value > cantidadMaximaDeRegistros) ? cantidadMaximaDeRegistros : value;
            }
        }
      
    }
}
