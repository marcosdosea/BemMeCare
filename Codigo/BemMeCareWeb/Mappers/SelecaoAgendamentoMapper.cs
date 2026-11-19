using BemMeCareWeb.Models;
using Core.DTO;
using Mapster;

namespace BemMeCareWeb.Mappers;

public static class SelecaoAgendamentoMapper
{
    public static SelecaoAgendamentoDTO ToEntity(this SelecaoAgendamentoViewModel viewModel)
    {
        return viewModel.Adapt<SelecaoAgendamentoDTO>();
    }

    public static SelecaoAgendamentoViewModel ToViewModel(this SelecaoAgendamentoDTO selecao)
    {
        return selecao.Adapt<SelecaoAgendamentoViewModel>();
    }
}
