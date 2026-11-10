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
        viewModel.Categorias = profissional.IdCategoria ?? Enumerable.Empty<Categorium>();
        viewModel.Servicos = profissional.Servicoprofissionals != null
            ? profissional.Servicoprofissionals
                .Where(sp => sp.IdServicoNavigation != null)
                .Select(sp => sp.IdServicoNavigation)
            : Enumerable.Empty<Servico>();
        viewModel.IdCategoriaSelecionada = profissional.IdCategoria?.FirstOrDefault()?.Id;
        viewModel.IdServicosSelecionados = profissional.Servicoprofissionals?.Select(sp => sp.IdServico) ?? Enumerable.Empty<uint>();

        return viewModel;
    }

    public static Profissional ToEntity(this ProfissionalViewModel viewModel)
    {
        var profissional = viewModel.Adapt<Profissional>();

        if (viewModel.IdCategoriaSelecionada.HasValue && viewModel.IdCategoriaSelecionada.Value > 0)
        {
            profissional.IdCategoria.Add(new Categorium
            {
                Id = viewModel.IdCategoriaSelecionada.Value,
                IdEstabelecimento = viewModel.IdEstabelecimento
            });
        }

        if (viewModel.IdServicosSelecionados != null)
        {
            foreach (var idServico in viewModel.IdServicosSelecionados)
            {
                profissional.Servicoprofissionals.Add(new Servicoprofissional
                {
                    IdProfissional = viewModel.IdPessoa,
                    IdEstabelecimento = viewModel.IdEstabelecimento,
                    IdServico = idServico
                });
            }
        }

        return profissional;
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