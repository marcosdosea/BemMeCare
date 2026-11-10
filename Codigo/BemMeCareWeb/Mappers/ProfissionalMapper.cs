using BemMeCareWeb.Models;
using Core;
using Mapster;

namespace BemMeCareWeb.Mappers;

public static class ProfissionalMapper
{
    public static ProfissionalViewModel ToViewModel(this Profissional profissional)
    {
        var viewModel = profissional.Adapt<ProfissionalViewModel>();

        viewModel.Nome = profissional.IdPessoaNavigation?.Nome;
        viewModel.Cpf = profissional.IdPessoaNavigation?.Cpf;
        viewModel.Categorias = profissional.IdCategoria;
        viewModel.Servicos = profissional.Servicoprofissionals
            .Where(sp => sp.IdServicoNavigation != null)
            .Select(sp => sp.IdServicoNavigation);
        viewModel.IdCategoriaSelecionada = profissional.IdCategoria.FirstOrDefault()?.Id;
        viewModel.IdServicosSelecionados = profissional.Servicoprofissionals.Select(sp => sp.IdServico);

        return viewModel;
    }

    public static Profissional ToEntity(this ProfissionalViewModel viewModel)
    {
        return viewModel.Adapt<Profissional>();
    }

    public static IEnumerable<ProfissionalViewModel> ToViewModel(this IEnumerable<Profissional> profissionais)
    {
        return profissionais.Select(profissional => profissional.ToViewModel());
    }

    public static IEnumerable<Profissional> ToEntity(this IEnumerable<ProfissionalViewModel> viewModels)
    {
        return viewModels.Adapt<IEnumerable<Profissional>>();
    }
}
