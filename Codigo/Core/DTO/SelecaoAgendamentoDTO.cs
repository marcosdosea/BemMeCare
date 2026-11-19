namespace Core.DTO
{
    public class SelecaoAgendamentoDTO
    {
        public uint IdProfissional { get; set; }

        public IEnumerable<uint> IdServicosSelecionados { get; set; } = new List<uint>();

        public DateOnly? DataDoAgendamento { get; set; }

        public TimeOnly? HorarioAgendamento { get; set; }
    }
}
