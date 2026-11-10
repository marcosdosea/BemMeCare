using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace BemMeCareWeb.Models;

public class CategoriaViewModel
{
    [Display(Name = "Código")]
    public uint Id { get; set; }

    [BindNever]
    public uint IdEstabelecimento { get; set; }

    [Display(Name = "Categoria")]
    [Required(ErrorMessage = "Nome da categoria é obrigatório")]
    [StringLength(45, MinimumLength = 3, ErrorMessage = "Nome da categoria deve ter entre 3 e 45 caracteres")]
    public string? Nome { get; set; }
}
