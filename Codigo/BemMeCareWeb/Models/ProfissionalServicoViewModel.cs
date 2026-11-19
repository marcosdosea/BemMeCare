using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace BemMeCareWeb.Models;

public class ProfissionalServicoViewModel
{
    [Display(Name = "Código do profissional")]
    public uint IdProfissional { get; set; }

    [Display(Name = "Código do serviço")]
    public uint IdServico { get; set; }

    [BindNever]
    public uint IdEstabelecimento { get; set; }

    [Display(Name = "Serviço")]
    [BindNever]
    public string? NomeServico { get; set; }

    [Display(Name = "Exige anamnese")]
    public bool ExigeAnamnese { get; set; }

    [Display(Name = "Link do formulário")]
    [Url(ErrorMessage = "Informe um link válido")]
    [StringLength(95, ErrorMessage = "O link deve ter no máximo 95 caracteres")]
    public string? FormAnamnese { get; set; }

    [Display(Name = "Preço")]
    [Required(ErrorMessage = "Preço é obrigatório")]
    [Range(typeof(decimal), "0", "100000.00",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true,
        ErrorMessage = "Preço deve ser maior ou igual a zero")]
    public decimal Preco { get; set; }

    [Display(Name = "Percentual do sinal")]
    [Required(ErrorMessage = "Percentual do sinal é obrigatório")]
    [Range(0, 100, ErrorMessage = "Percentual do sinal deve estar entre 0 e 100")]
    public float PercentualSinal { get; set; }

    [Display(Name = "Quando pagar o sinal")]
    [Required(ErrorMessage = "Quando pagar o sinal é obrigatório")]
    [RegularExpression("^(PRIMEIRO_AGENDAMENTO|TODO_AGENDAMENTO)$", ErrorMessage = "Informe uma opção válida para o pagamento do sinal")]
    public string QuandoPagarSinal { get; set; } = "PRIMEIRO_AGENDAMENTO";

    [Display(Name = "Duração (minutos)")]
    [Required(ErrorMessage = "Duração é obrigatória")]
    [Range(0, 1440, ErrorMessage = "Duração deve estar entre 0 e 1440 minutos")]
    public uint Duracao { get; set; }
}