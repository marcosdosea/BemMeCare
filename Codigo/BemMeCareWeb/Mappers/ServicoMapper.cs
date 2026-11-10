using BemMeCareWeb.Models;
using Core;
using Mapster;

namespace BemMeCareWeb.Mappers
{
    public static class ServicoMapper
    {
        public static ServicoViewModel ToViewModel(this Servico servico)
        {
            return servico.Adapt<ServicoViewModel>();
        }
        public static Servico ToEntity(this ServicoViewModel viewModel)
        {
            return viewModel.Adapt<Servico>();
        }
        public static IEnumerable<ServicoViewModel> ToViewModel(this IEnumerable<Servico> servicos)
        {
            return servicos.Adapt<IEnumerable<ServicoViewModel>>();
        }
        public static IEnumerable<Servico> ToEntity(this IEnumerable<ServicoViewModel> viewModels)
        {
            return viewModels.Adapt<IEnumerable<Servico>>();
        }
    }
}
