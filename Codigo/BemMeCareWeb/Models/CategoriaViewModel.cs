using System.ComponentModel.DataAnnotations;

namespace BemMeCareWeb.Models;

public class CategoriaViewModel
{
    [Display(Name = "Código")]
    [Required(ErrorMessage = "Código da categoria é obrigatório")]
    public uint Id { get; set; }
    
    [Display(Name = "Código do estabelecimento")]
    [Required(ErrorMessage = "Código do estabelecimento é obrigatório")]
    public uint IdEstabelecimento { get; set; }
    
    [Display(Name = "Nome da categoria")]
    [Required(ErrorMessage = "Nome da categoria é obrigatório")]
    [StringLength(45, MinimumLength = 3, ErrorMessage = "Nome da categoria deve ter entre 3 e 45 caracteres")]
    public string? Nome { get; set; }
}
