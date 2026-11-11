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
        viewModel.IdCategoriasSelecionadas = profissional.IdCategoria?.Select(c => c.Id) ?? Enumerable.Empty<uint>();
        viewModel.IdServicosSelecionados = profissional.Servicoprofissionals?.Select(sp => sp.IdServico) ?? Enumerable.Empty<uint>();

        return viewModel;
    }

    public static Profissional ToEntity(this ProfissionalViewModel viewModel)
    {
        var profissional = viewModel.Adapt<Profissional>();

        if (viewModel.IdCategoriasSelecionadas != null)
        {
            foreach (var idCategoria in viewModel.IdCategoriasSelecionadas)
            {
                profissional.IdCategoria.Add(new Categorium
                {
                    Id = idCategoria,
                    IdEstabelecimento = viewModel.IdEstabelecimento
                });
            }
        }

        if (viewModel.IdServicosSelecionados != null)
        {
            foreach (var idServico in viewModel.IdServicosSelecionados)
            {
                profissional.Servicoprofissionals.Add(new Servicoprofissional
                {
                    IdProfissional = viewModel.IdPessoa,
                    IdEstabelecimento = viewModel.IdEstabelecimento,
                    IdServico = idServico,
                    ExigeAnamnese = 0,
                    FormAnamnese = string.Empty,
                    Preco = 0m,
                    PercentualSinal = 0f,
                    QuandoPagarSinal = "PRIMEIRO_AGENDAMENTO",
                    Duracao = 0
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