using BemMeCareWeb.Models;
using Core;
using Mapster;

namespace BemMeCareWeb.Mappers;

public static class ProfissionalServicoMapper
{
    public static ProfissionalServicoViewModel ToViewModel(this Servicoprofissional profissionalServico)
    {
        var viewModel = profissionalServico.Adapt<ProfissionalServicoViewModel>();
        viewModel.NomeServico = profissionalServico.IdServicoNavigation?.Nome;
        return viewModel;
    }

    public static Servicoprofissional ToEntity(this ProfissionalServicoViewModel viewModel)
    {
        return viewModel.Adapt<Servicoprofissional>();
    }

    public static IEnumerable<ProfissionalServicoViewModel> ToViewModel(this IEnumerable<Servicoprofissional> profissionalServicos)
    {
        return profissionalServicos.Select(profissionalServico => profissionalServico.ToViewModel());
    }

    public static IEnumerable<Servicoprofissional> ToEntity(this IEnumerable<ProfissionalServicoViewModel> viewModels)
    {
        return viewModels.Adapt<IEnumerable<Servicoprofissional>>();
    }
}