using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BemMeCareWeb.Models;

public class HorarioViewModel : IValidatableObject
{
    [Display(Name = "Código")]
    public uint Id { get; set; }

    [Display(Name = "Início do Turno")]
    [Required(ErrorMessage = "O início do turno é obrigatório.")]
    [DataType(DataType.Time)]
    public TimeSpan InicioTurno { get; set; }

    [Display(Name = "Fim do Turno")]
    [Required(ErrorMessage = "O fim do turno é obrigatório.")]
    [DataType(DataType.Time)]
    public TimeSpan FimTurno { get; set; }

    [Display(Name = "Início da Vigência")]
    [Required(ErrorMessage = "A data de início da vigência é obrigatória.")]
    [DataType(DataType.Date)]
    public DateTime InicioVigencia { get; set; } = DateTime.Today;

    [Display(Name = "Fim da Vigência")]
    [DataType(DataType.Date)]
    public DateTime? FimVigencia { get; set; }

    [Display(Name = "Dia da Semana")]
    [Required(ErrorMessage = "O dia da semana é obrigatório.")]
    [StringLength(20, ErrorMessage = "O dia da semana deve ter entre 3 e 20 caracteres.")]
    public string DiaSemana { get; set; } = string.Empty;

    [Display(Name = "Profissional")]
    [Required(ErrorMessage = "O profissional é obrigatório.")]
    public uint IdProfissional { get; set; }

    [Display(Name = "Estabelecimento")]
    [BindNever]
    public uint IdEstabelecimento { get; set; }

    [Display(Name = "Nome do Profissional")]
    public string? NomeProfissional { get; set; }

    [Display(Name = "Nome do Estabelecimento")]
    public string? NomeEstabelecimento { get; set; }

    /// <summary>
    /// Valida as regras de consistência temporal do turno e do período de vigência.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FimTurno <= InicioTurno)
        {
            yield return new ValidationResult(
                "O fim do turno deve ser posterior ao início do turno.",
                new[] { nameof(FimTurno) });
        }

        if (FimVigencia.HasValue && FimVigencia.Value.Date <= InicioVigencia.Date)
        {
            yield return new ValidationResult(
                "A data de fim da vigência deve ser posterior à data de início (com no mínimo 1 dia de diferença).",
                new[] { nameof(FimVigencia) });
        }
    }
}
