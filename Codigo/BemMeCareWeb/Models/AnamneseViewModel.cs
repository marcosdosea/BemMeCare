using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace BemMeCareWeb.Models;

public class AnamneseViewModel
{
    public uint IdProfissional { get; set; }
    public uint IdServico { get; set; }

    [BindNever]
    public uint IdEstabelecimento { get; set; }

    [Display(Name = "Exige anamnese")]
    public bool ExigeAnamnese { get; set; }

    [Display(Name = "Link do formulário")]
    [Required(ErrorMessage = "O link do formulário é obrigatório")]
    [Url(ErrorMessage = "Informe um link válido")]
    [StringLength(95, ErrorMessage = "O link deve ter no máximo 95 caracteres")]
    public string? FormAnamnese { get; set; }
}