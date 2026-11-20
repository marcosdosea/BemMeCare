namespace Core.DTO
{
    public class AgendamentoDTO
    {
        public string NomeProfissional { get; set; } = string.Empty;
        public IEnumerable<string> Servicos { get; set; } = [];
        public decimal PrecoTotal { get; set; }
        public uint DuracaoTotalEmMinutos { get; set; }
        public DateTime HorarioAtendimento { get; set; }
    }
}
