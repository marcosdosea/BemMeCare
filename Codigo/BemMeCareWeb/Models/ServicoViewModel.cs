using System.ComponentModel.DataAnnotations;


namespace BemMeCareWeb.Models
{
    public class ServicoViewModel
    {
        [Display(Name = "Código")]
        [Key]
        [Required(ErrorMessage = "Código do serviço é obrigatório")]
        public uint Id { get; set; }

        [Display(Name = "Código do estabelecimento")]
        [Required(ErrorMessage = "Código do estabelecimento é obrigatório")]
        public uint IdEstabelecimento { get; set; }

        [Display(Name = "Código da categoria")]
        [Required(ErrorMessage = "Código da categoria é obrigatório")]
        public uint IdCategoria { get; set; }

        [Display(Name = "Nome do serviço")]
        [Required(ErrorMessage = "Nome do serviço é obrigatório")]
        [StringLength(45, MinimumLength = 3, ErrorMessage = "Nome do serviço deve ter entre 3 e 45 caracteres")]
        public string? Nome { get; set; }
    }
}
