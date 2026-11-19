using BemMeCareWeb.Models;
using Core.DTO;
using Mapster;

namespace BemMeCareWeb.Mappers;

public static class AgendamentoResumoMapper
{
    public static AgendamentoResumoDTO ToEntity(this AgendamentoResumoViewModel viewModel)
    {
        return viewModel.Adapt<AgendamentoResumoDTO>();
    }

    public static AgendamentoResumoViewModel ToViewModel(this AgendamentoResumoDTO selecao)
    {
        return selecao.Adapt<AgendamentoResumoViewModel>();
    }
}
