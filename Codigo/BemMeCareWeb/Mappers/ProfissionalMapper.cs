using BemMeCareWeb.Models;
using Core;
using Mapster;

namespace BemMeCareWeb.Mappers
{
    public class ProfissionalMapper
    {
        public static ProfissionalViewModel ToViewModel(Profissional profissional)
        {
            return profissional.Adapt<ProfissionalViewModel>();
        }

        public static Profissional ToEntity(ProfissionalViewModel profissionalViewModel)
        {
            return profissionalViewModel.Adapt<Profissional>();
        }

        public static IEnumerable<ProfissionalViewModel> ToViewModel(IEnumerable<Profissional> profissionais)
        {
            return profissionais.Adapt<IEnumerable<ProfissionalViewModel>>();
        }

        public static IEnumerable<Profissional> ToEntity(IEnumerable<ProfissionalViewModel> profissionaisViewModel)
        {
            return profissionaisViewModel.Adapt<IEnumerable<Profissional>>();
        }
    }
}
