using BemMeCareWeb.Models;
using Core;
using Mapster;

namespace BemMeCareWeb.Mappers;

public static class PessoaMapper
{
    public static PessoaViewModel ToViewModel(this Pessoa pessoa)
    {
        return pessoa.Adapt<PessoaViewModel>();
    }

    public static Pessoa ToEntity(this PessoaViewModel viewModel)
    {
        return viewModel.Adapt<Pessoa>();
    }

    public static IEnumerable<PessoaViewModel> ToViewModel(this IEnumerable<Pessoa> pessoas)
    {
        return pessoas.Adapt<IEnumerable<PessoaViewModel>>();
    }

    public static IEnumerable<Pessoa> ToEntity(this IEnumerable<PessoaViewModel> viewModels)
    {
        return viewModels.Adapt<IEnumerable<Pessoa>>();
    }
}

