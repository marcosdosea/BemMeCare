using BemMeCareWeb.Models;
using Core.DTO;
using Mapster;

namespace BemMeCareWeb.Mappers;

public static class AgendamentoServicosMapper
{
    public static AgendamentoServicosViewModel ToViewModel(this IEnumerable<ProfissionalServicosDTO> profissionais)
    {
        var viewModel = new AgendamentoServicosViewModel();

        viewModel.Profissionais = profissionais
            .Select(p => p.ToProfissionalItemViewModel())
            .ToList();

        return viewModel;
    }

    public static ProfissionalItemViewModel ToProfissionalItemViewModel(this ProfissionalServicosDTO profissional)
    {
        var viewModel = profissional.Adapt<ProfissionalItemViewModel>();

        viewModel.Servicos = profissional.Servicos
            .Select(s => s.ToServicoProfissionalItemViewModel())
            .ToList();

        return viewModel;
    }

    public static ServicoProfissionalItemViewModel ToServicoProfissionalItemViewModel(this ServicoProfissionalDTO servico)
    {
        return servico.Adapt<ServicoProfissionalItemViewModel>();
    }
}
