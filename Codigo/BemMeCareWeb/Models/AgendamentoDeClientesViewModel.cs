using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BemMeCareWeb.Models
{
    public class AgendamentoDeClientesViewModel
    {
        [Display(Name = "Código")]
        public uint Id { get; set; }

        [Display(Name = "Data e horário")]
        [Required(ErrorMessage = "A data e o horário são obrigatórios.")]
        public DateTime? DataHoraAtendimento { get; set; }

        [Display(Name = "Observações")]
        [StringLength(100, ErrorMessage = "As observações devem ter no máximo 100 caracteres.")]
        public string? Observacoes { get; set; }

        [Display(Name = "Serviços")]
        [MinLength(1, ErrorMessage = "Selecione ao menos um serviço.")]
        public List<uint> IdsServicos { get; set; } = [];

        [Display(Name = "Profissional")]
        [Range(1, uint.MaxValue, ErrorMessage = "Selecione um profissional.")]
        public uint IdProfissional { get; set; }

        [Display(Name = "Nome do cliente")]
        [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
        [StringLength(45, ErrorMessage = "O nome do cliente deve ter no máximo 45 caracteres.")]
        public string NomeCliente { get; set; } = string.Empty;

        [Display(Name = "Telefone do cliente")]
        [Required(ErrorMessage = "O telefone do cliente é obrigatório.")]
        public string TelefoneCliente { get; set; } = string.Empty;

        [Display(Name = "Nome do profissional")]
        public string? NomeProfissional { get; set; }

        [Display(Name = "Serviços selecionados")]
        public string? NomesServicos { get; set; }

        [Display(Name = "Preço final")]
        public decimal PrecoFinal { get; set; }

        [Display(Name = "Duração (minutos)")]
        public uint DuracaoMinutos { get; set; }

        [Display(Name = "Status do agendamento")]
        public string? StatusAgendamento { get; set; }

        [Display(Name = "Status do pagamento")]
        public string? StatusPagamento { get; set; }

        [Display(Name = "Valor pago de sinal")]
        public decimal ValorPagoSinal { get; set; }

        [BindNever]
        public uint IdPessoaCliente { get; set; }

        [BindNever]
        public uint IdEstabelecimento { get; set; }

        [BindNever]
        public uint IdPessoaMarcou { get; set; }

        [BindNever]
        public DateTime DataHoraCriacao { get; set; }

        [BindNever]
        public DateTime DataHoraFimAtendimento { get; set; }

        [Display(Name = "Profissionais disponíveis")]
        [BindNever]
        public IEnumerable<SelectListItem> ProfissionaisDisponiveis { get; set; } = [];

        [Display(Name = "Serviços disponíveis")]
        [BindNever]
        public IEnumerable<SelectListItem> ServicosDisponiveis { get; set; } = [];
    }
}