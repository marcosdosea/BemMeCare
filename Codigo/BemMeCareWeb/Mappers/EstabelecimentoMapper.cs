using BemMeCareWeb.Models;
using Core;
using Mapster;

namespace BemMeCareWeb.Mappers;

public static class EstabelecimentoMapper
{
    public static EstabelecimentoViewModel ToViewModel(this Estabelecimento estabelecimento)
    {
        return estabelecimento.Adapt<EstabelecimentoViewModel>();
    }

    public static Estabelecimento ToEntity(this EstabelecimentoViewModel viewModel)
    {
        return viewModel.Adapt<Estabelecimento>();
    }

    public static IEnumerable<EstabelecimentoViewModel> ToViewModel(this IEnumerable<Estabelecimento> estabelecimentos)
    {
        return estabelecimentos.Adapt<IEnumerable<EstabelecimentoViewModel>>();
    }

    public static IEnumerable<Estabelecimento> ToEntity(this IEnumerable<EstabelecimentoViewModel> viewModels)
    {
        return viewModels.Adapt<IEnumerable<Estabelecimento>>();
    }
}