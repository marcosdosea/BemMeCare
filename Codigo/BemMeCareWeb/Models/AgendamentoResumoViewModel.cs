namespace BemMeCareWeb.Models;

public class AgendamentoResumoViewModel
{
    public SelecaoAgendamentoViewModel Selecao { get; set; } = new();
    public string NomeProfissional { get; set; } = string.Empty;
    public IEnumerable<ServicoProfissionalItemViewModel> Servicos { get; set; } = [];
    public decimal PrecoTotal { get; set; }
    public uint DuracaoTotalEmMinutos { get; set; }
    public TimeOnly HorarioAtendimento { get; set; }
}
