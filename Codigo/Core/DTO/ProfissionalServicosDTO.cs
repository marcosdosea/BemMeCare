namespace Core.DTO
{
    public class ProfissionalServicosDTO
    {
        public uint Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public IEnumerable<ServicoProfissionalDTO> Servicos { get; set; } = new List<ServicoProfissionalDTO>();
    }

    public class ServicoProfissionalDTO
    {
        public uint Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public decimal Preco { get; set; }
        public uint DuracaoEmMinutos { get; set; }
    }
}
