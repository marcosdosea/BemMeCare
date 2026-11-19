using Microsoft.AspNetCore.Mvc.Rendering;

namespace BemMeCareWeb.Models;

public class AgendamentoDeClientesAgendaViewModel
{
    public DateTime DataReferencia { get; set; }

    public DateTime DataAnterior { get; set; }

    public DateTime DataProxima { get; set; }

    public string Visualizacao { get; set; } = string.Empty;

    public string TituloPeriodo { get; set; } = string.Empty;

    public uint? IdProfissional { get; set; }

    public IEnumerable<SelectListItem> ProfissionaisDisponiveis { get; set; } = [];

    public IReadOnlyList<IReadOnlyList<AgendamentoDiaViewModel>> Semanas { get; set; } = [];
}

public class AgendamentoDiaViewModel
{
    public DateTime Data { get; set; }

    public bool PertenceAoMesAtual { get; set; }

    public IReadOnlyList<AgendamentoDeClientesViewModel> Agendamentos { get; set; } = [];
}
