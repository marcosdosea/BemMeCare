using BemMeCareWeb.Models;
using Core;
using Mapster;

namespace BemMeCareWeb.Mappers;

public static class CategoriaProfile
{
    public static CategoriaViewModel ToViewModel(this Categorium categoria)
    {
        return categoria.Adapt<CategoriaViewModel>();
    }

    public static Categorium ToEntity(this CategoriaViewModel viewModel)
    {
        return viewModel.Adapt<Categorium>();
    }

    public static IEnumerable<CategoriaViewModel> ToViewModel(this IEnumerable<Categorium> categorias)
    {
        return categorias.Adapt<IEnumerable<CategoriaViewModel>>();
    }

    public static IEnumerable<Categorium> ToEntity(this IEnumerable<CategoriaViewModel> viewModels)
    {
        return viewModels.Adapt<IEnumerable<Categorium>>();
    }
}