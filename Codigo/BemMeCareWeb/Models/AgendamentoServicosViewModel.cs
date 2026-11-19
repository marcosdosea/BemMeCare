namespace BemMeCareWeb.Models;

public class AgendamentoServicosViewModel
{
    public SelecaoAgendamentoViewModel Selecao { get; set; } = new();

    public IEnumerable<ProfissionalItemViewModel> Profissionais { get; set; } = [];
}

public class ProfissionalItemViewModel
{
    public uint Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public IEnumerable<ServicoProfissionalItemViewModel> Servicos { get; set; } = [];
}

public class ServicoProfissionalItemViewModel
{
    public uint Id { get; set; }
    public string Nome { get; set; } = string.Empty;

    public decimal Preco { get; set; }
    public uint DuracaoEmMinutos { get; set; }
}