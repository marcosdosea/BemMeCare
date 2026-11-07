using System.ComponentModel.DataAnnotations;

namespace BemMeCareWeb.Models;

public class EstabelecimentoViewModel
{
    [Display(Name = "Código")]
    [Key]
    [Required(ErrorMessage = "Código do estabelecimento é obrigatório")]
    public uint Id { get; set; }

    [Display(Name = "Nome")]
    [Required(ErrorMessage = "Nome do estabelecimento é obrigatório")]
    [StringLength(45, MinimumLength = 3, ErrorMessage = "Nome do estabelecimento deve ter entre 3 e 45 caracteres")]
    public string? Nome { get; set; }

    [Display(Name = "Modalidade")]
    [Required(ErrorMessage = "Modalidade do estabelecimento é obrigatório")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Modalidade do estabelecimento deve ter entre 3 e 50 caracteres")]
    public string? Modalidade { get; set; }

    [Display(Name = "CNPJ")]
    [Required(ErrorMessage = "CNPJ do estabelecimento é obrigatório")]
    [StringLength(14, MinimumLength = 14, ErrorMessage = "CNPJ do estabelecimento deve ter 14 caracteres")]
    public string? Cnpj { get; set; }

    [Display(Name = "CEP")]
    [Required(ErrorMessage = "CEP do estabelecimento é obrigatório")]
    [StringLength(8, MinimumLength = 8, ErrorMessage = "CEP do estabelecimento deve ter 8 caracteres")]
    public string? CEP { get; set; }

    [Display(Name = "Rua")]
    [Required(ErrorMessage = "Rua do estabelecimento é obrigatório")]
    [StringLength(45, MinimumLength = 3, ErrorMessage = "Rua do estabelecimento deve ter entre 3 e 45 caracteres")]
    public string? Rua { get; set; }

    [Display(Name = "Número")]
    [Required(ErrorMessage = "Número do estabelecimento é obrigatório")]
    [StringLength(45, MinimumLength = 1, ErrorMessage = "Número do estabelecimento deve ter entre 1 e 45 caracteres")]
    public string? Numero { get; set; }

    [Display(Name = "Complemento")]
    [StringLength(45, MinimumLength = 3, ErrorMessage = "Complemento do estabelecimento deve ter entre 3 e 45 caracteres")]
    public string? Complemento { get; set; }

    [Display(Name = "Bairro")]
    [Required(ErrorMessage = "Bairro do estabelecimento é obrigatório")]
    [StringLength(45, MinimumLength = 3, ErrorMessage = "Bairro do estabelecimento deve ter entre 3 e 45 caracteres")]
    public string? Bairro { get; set; }

    [Display(Name = "Cidade")]
    [Required(ErrorMessage = "Cidade do estabelecimento é obrigatória")]
    [StringLength(45, MinimumLength = 3, ErrorMessage = "Cidade do estabelecimento deve ter entre 3 e 45 caracteres")]
    public string? Cidade { get; set; }

    [Display(Name = "Estado")]
    [Required(ErrorMessage = "Estado do estabelecimento é obrigatório")]
    [StringLength(45, MinimumLength = 2, ErrorMessage = "Estado do estabelecimento deve ter entre 2 e 45 caracteres")]
    public string? Estado { get; set; }

    [Display(Name = "Contato")]
    [Required(ErrorMessage = "Contato do estabelecimento é obrigatório")]
    [StringLength(45, MinimumLength = 3, ErrorMessage = "Contato do estabelecimento deve ter entre 3 e 45 caracteres")]
    public string? Contato { get; set; }

    [Display(Name = "Chave Pix")]
    [Required(ErrorMessage = "Chave Pix do estabelecimento é obrigatória")]
    [StringLength(45, MinimumLength = 3, ErrorMessage = "Chave Pix do estabelecimento deve ter entre 3 e 45 caracteres")]
    public string? ChavePix { get; set; }

    [Display(Name = "Foto")]
    public byte[]? Foto { get; set; }

    [Display(Name = "Código da pessoa")]
    [Required(ErrorMessage = "Código da pessoa é obrigatório")]
    public uint IdPessoa { get; set; }

}
