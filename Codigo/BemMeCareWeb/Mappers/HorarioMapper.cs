using BemMeCareWeb.Models;
using Core;
using Mapster;

namespace BemMeCareWeb.Mappers;

public static class HorarioMapper
{
    public static HorarioViewModel ToViewModel(this Horario horario)
    {
        return horario.Adapt<HorarioViewModel>();
    }

    public static Horario ToEntity(this HorarioViewModel viewModel)
    {
        return viewModel.Adapt<Horario>();
    }

    public static IEnumerable<HorarioViewModel> ToViewModel(this IEnumerable<Horario> horarios)
    {
        return horarios.Adapt<IEnumerable<HorarioViewModel>>();
    }

    public static IEnumerable<Horario> ToEntity(this IEnumerable<HorarioViewModel> viewModels)
    {
        return viewModels.Adapt<IEnumerable<Horario>>();
    }
}
