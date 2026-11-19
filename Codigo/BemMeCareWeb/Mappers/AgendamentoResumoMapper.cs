using BemMeCareWeb.Models;
using Core.DTO;
using Mapster;

namespace BemMeCareWeb.Mappers;

public static class AgendamentoResumoMapper
{
    public static AgendamentoResumoDTO ToEntity(this AgendamentoHorarioViewModel viewModel)
    {
        return viewModel.Adapt<AgendamentoResumoDTO>();
    }

    public static AgendamentoHorarioViewModel ToViewModel(this AgendamentoResumoDTO selecao)
    {
        return selecao.Adapt<AgendamentoHorarioViewModel>();
    }
}
