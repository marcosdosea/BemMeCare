using BemMeCareWeb.Models;
using Core;
using Mapster;

namespace BemMeCareWeb.Mappers;

public static class EstabelecimentoMapper
{
    public static EstabelecimentoViewModel ToViewModel(Estabelecimento estabelecimento)
    {
        return estabelecimento.Adapt<EstabelecimentoViewModel>();
    }
    public static Estabelecimento ToEntity(EstabelecimentoViewModel viewModel)
    {
        return viewModel.Adapt<Estabelecimento>();
    }
    public static IEnumerable<EstabelecimentoViewModel> ToViewModel(IEnumerable<Estabelecimento> estabelecimentos)
    {
        return estabelecimentos.Adapt<IEnumerable<EstabelecimentoViewModel>>();
    }
    public static IEnumerable<Estabelecimento> ToEntity(IEnumerable<EstabelecimentoViewModel> viewModels)
    {
        return viewModels.Adapt<IEnumerable<Estabelecimento>>();
    }
}