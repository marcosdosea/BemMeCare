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

    public static Pessoa ToEntity(this PessoaViewModel pessoaViewModel)
    {
        var pessoa = pessoaViewModel.Adapt<Pessoa>();
        if (pessoa.Cpf != null)
            pessoa.Cpf = pessoa.Cpf.Replace(".", "").Replace("-", "");
        if (pessoa.Cep != null)
            pessoa.Cep = pessoa.Cep.Replace("-", "");
        return pessoa;
    }

    public static IEnumerable<PessoaViewModel> ToViewModel(this IEnumerable<Pessoa> pessoas)
    {
        return pessoas.Adapt<IEnumerable<PessoaViewModel>>();
    }

    public static IEnumerable<Pessoa> ToEntity(this IEnumerable<PessoaViewModel> pessoaViewModel)
    {
        return pessoaViewModel.Adapt<IEnumerable<Pessoa>>();
    }
}
