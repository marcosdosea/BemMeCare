using System.ComponentModel.DataAnnotations;

namespace BemMeCareWeb.Models
{
	public class ProfissionalViewModel
	{
		[Display(Name = "Pessoa")]
		[Key]
		[Required(ErrorMessage = "O campo {0} é obrigatório.")]
		public uint IdPessoa { get; set; }

		[Display(Name = "Estabelecimento")]
		[Required(ErrorMessage = "O campo {0} é obrigatório.")]
		public uint IdEstabelecimento { get; set; }

		[Display(Name = "Gerir Horário")]
		[Required(ErrorMessage = "O campo {0} é obrigatório.")]
		public sbyte GerirHorario { get; set; }

		[Display(Name = "Está Ativo")]
		[Required(ErrorMessage = "O campo {0} é obrigatório.")]
		public sbyte EstaAtivo { get; set; }

		[Display(Name = "Gerir Serviço")]
		[Required(ErrorMessage = "O campo {0} é obrigatório.")]
		public sbyte GerirServico { get; set; }

		[Display(Name = "Receber Pagamento Direto")]
		[Required(ErrorMessage = "O campo {0} é obrigatório.")]
		public sbyte ReceberPagamentoDireto { get; set; }

		[Display(Name = "Chave PIX")]
		[StringLength(45, ErrorMessage = "O campo {0} deve ter no máximo {1} caracteres.")]
		public string? ChavePix { get; set; }

		[Display(Name = "Duração do Intervalo de Tempo")]
		[Required(ErrorMessage = "O campo {0} é obrigatório.")]
		public uint DuracaoIntervaloTempo { get; set; }

		[Display(Name = "É Profissional")]
		public sbyte? EProfissional { get; set; }

		[Display(Name = "É Atendente")]
		public sbyte? EAtendente { get; set; }
	}
}