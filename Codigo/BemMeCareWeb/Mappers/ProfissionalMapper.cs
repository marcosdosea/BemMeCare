using BemMeCareWeb.Models;
using Core;
using Mapster;

namespace BemMeCareWeb.Mappers
{
    public static class ProfissionalMapper
    {
        public static ProfissionalViewModel? ToViewModel(this Profissional? entity)
        {
            if (entity == null) return null;
            var viewModel = entity.Adapt<ProfissionalViewModel>();
            viewModel.Nome = entity.IdPessoaNavigation?.Nome;
            viewModel.Cpf = entity.IdPessoaNavigation?.Cpf;
            viewModel.Categorias = entity.IdCategoria != null && entity.IdCategoria.Any()
                ? string.Join(", ", entity.IdCategoria.Select(c => c.Nome))
                : null;
            viewModel.Servicos = entity.Servicoprofissionals != null && entity.Servicoprofissionals.Any()
                ? string.Join(", ", entity.Servicoprofissionals
                    .Where(sp => sp.IdServicoNavigation != null)
                    .Select(sp => sp.IdServicoNavigation.Nome))
                : null;
            return viewModel;
        }

        public static List<ProfissionalViewModel> ToViewModel(this IEnumerable<Profissional> entities)
        {
            return entities.Select(e => e.ToViewModel()!).ToList();
        }

        public static Profissional ToEntity(this ProfissionalViewModel viewModel)
        {
            return viewModel.Adapt<Profissional>();
        }
    }
}
