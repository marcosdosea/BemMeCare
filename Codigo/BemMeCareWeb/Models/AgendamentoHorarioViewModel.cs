namespace BemMeCareWeb.Models;

public class AgendamentoHorarioViewModel
{
    public SelecaoAgendamentoViewModel Selecao { get; set; } = new();
    public string NomeProfissional { get; set; } = string.Empty;
    public IEnumerable<TimeOnly> Horarios { get; set; } = [];
    public string Mensagem { get; set; } = string.Empty;
    public DateOnly DataLimiteInferior { get; set; }
}
