using BemMeCareWeb.Models;
using Core;
using Mapster;

namespace BemMeCareWeb.Mappers;
public static class AnamneseMapper
{
    public static AnamneseViewModel ToViewModel(this Servicoprofissional anamnese)
    {
        return anamnese.Adapt<AnamneseViewModel>();
    }

    public static Servicoprofissional ToEntity(this AnamneseViewModel viewModel)
    {
        return viewModel.Adapt<Servicoprofissional>();
    }

    public static IEnumerable<AnamneseViewModel> ToViewModel(this IEnumerable<Servicoprofissional> anamneses)
    {
        return anamneses.Adapt<IEnumerable<AnamneseViewModel>>();
    }

    public static IEnumerable<Servicoprofissional> ToEntity(this IEnumerable<AnamneseViewModel> viewModels)
    {
        return viewModels.Adapt<IEnumerable<Servicoprofissional>>();
    }
}
