using System.ComponentModel.DataAnnotations;
using Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BemMeCareWeb.Models
{
    public class ProfissionalViewModel
    {
        [Display(Name = "Código")]
        [Required(ErrorMessage = "O campo {0} é obrigatório.")]
        public uint IdPessoa { get; set; }

        [Display(Name = "Código do estabelecimento")]
        [BindNever]
        public uint IdEstabelecimento { get; set; }

        [Display(Name = "Profissional pode gerir o seu horário")]
        public sbyte GerirHorario { get; set; }

        [Display(Name = "Status")]
        public sbyte EstaAtivo { get; set; }

        [Display(Name = "Profissional pode gerir seus serviços")]
        public sbyte GerirServico { get; set; }

        [Display(Name = "Profissional recebe o pagamento do sinal")]
        public sbyte ReceberPagamentoDireto { get; set; }

        [Display(Name = "Chave PIX")]
        [StringLength(45, MinimumLength = 11, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres.")]
        public string? ChavePix { get; set; }

        [Display(Name = "Duração do intervalo de tempo")]
        public uint DuracaoIntervaloTempo { get; set; }

        [Display(Name = "É profissional")]
        public sbyte? EProfissional { get; set; }

        [Display(Name = "É atendente")]
        public sbyte? EAtendente { get; set; }

        [Display(Name = "Nome")]
        [BindNever]
        public string? Nome { get; set; }

        [Display(Name = "CPF")]
        public string? Cpf { get; set; }

        [Display(Name = "Categoria(s)")]
        public IEnumerable<uint> IdCategoriasSelecionadas { get; set; } = new List<uint>();

        [Display(Name = "Serviços")]
        public IEnumerable<uint> IdServicosSelecionados { get; set; } = new List<uint>();

        [Display(Name = "Categoria(s)")]
        [BindNever]
        public IEnumerable<Categorium> Categorias { get; set; } = Enumerable.Empty<Categorium>();

        [Display(Name = "Serviços")]
        [BindNever]
        public IEnumerable<Servico> Servicos { get; set; } = Enumerable.Empty<Servico>();

        [BindNever]
        public MultiSelectList? ListaCategorias { get; set; }

        [BindNever]
        public IEnumerable<Servico> ServicosDisponiveis { get; set; } = Enumerable.Empty<Servico>();
    }
}