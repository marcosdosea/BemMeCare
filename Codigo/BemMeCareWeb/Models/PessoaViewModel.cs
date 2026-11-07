using System.ComponentModel.DataAnnotations;

namespace BemMeCareWeb.Models;

public class PessoaViewModel
{
    [Display(Name = "Código")]
    [Key]
    [Required(ErrorMessage = "Código da pessoa é obrigatório")]
    public uint Id { get; set; }
    
    [Display(Name = "Nome da pessoa")]
    [Required(ErrorMessage = "Nome da pessoa é obrigatório")]
    [StringLength(45, MinimumLength = 3, ErrorMessage = "Nome da pessoa deve ter entre 3 e 45 caracteres")]
    public string? Nome { get; set; }

    [Display(Name = "Email")]
    [Required(ErrorMessage = "O e-mail é obrigatório")]
    [DataType(DataType.EmailAddress)]
    [EmailAddress(ErrorMessage = "E-mail inválido")]
    public string? Email { get; set; }

    [Display(Name = "Senha")]
    [Required(ErrorMessage = "A senha é obrigatória")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres")]
    public string? Senha { get; set; }

    [Display(Name = "CPF")]
    [Required(ErrorMessage = "O CPF é obrigatório")]
    [StringLength(14, MinimumLength = 14, ErrorMessage = "O CPF deve ter 14 caracteres")]
    public string? Cpf { get; set; }

    [Display(Name = "Telefone")]
    [Required(ErrorMessage = "O telefone é obrigatório")]
    [StringLength(15, MinimumLength = 10, ErrorMessage = "O telefone deve ter entre 10 e 15 caracteres")]
    public string? Telefone { get; set; }

    [Display(Name = "Data de Nascimento")]
    [DataType(DataType.Date)]
    [Required(ErrorMessage = "A data de nascimento é obrigatória")]
    public DateTime DataDeNascimento { get; set; }

    [Display(Name = "Cor")]
    [Required(ErrorMessage = "A cor é obrigatória")]
    public string? Cor { get; set; }

    [Display(Name = "Gênero")]
    [Required(ErrorMessage = "O gênero é obrigatório")]
    public string? Genero { get; set; }

    [Display(Name = "Sexualidade")]
    [Required(ErrorMessage = "A sexualidade é obrigatória")]
    public string? Sexualidade { get; set; }

    [Display(Name = "Foto")]
    public string? Foto { get; set; }

    [Display(Name = "CEP")]
    [Required(ErrorMessage = "O CEP é obrigatório")]
    [StringLength(9, MinimumLength = 9, ErrorMessage = "O CEP deve ter 9 caracteres")]
    public string? Cep { get; set; }

    [Display(Name = "Rua")]
    [Required(ErrorMessage = "A rua é obrigatória")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "A rua deve ter entre 3 e 100 caracteres")]
    public string? Rua { get; set; }

    [Display(Name = "Bairro")]
    [Required(ErrorMessage = "O bairro é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O bairro deve ter entre 3 e 100 caracteres")]
    public string? Bairro { get; set; }

    [Display(Name = "Número")]
    [Required(ErrorMessage = "O número é obrigatório")]
    [StringLength(10, MinimumLength = 1, ErrorMessage = "O número deve ter entre 1 e 10 caracteres")]
    public string? Numero { get; set; }

    [Display(Name = "Complemento")]
    [Required(ErrorMessage = "O complemento é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O complemento deve ter entre 3 e 100 caracteres")]
    public string? Complemento { get; set; }

    [Display(Name = "Cidade")]
    [Required(ErrorMessage = "A cidade é obrigatória")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "A cidade deve ter entre 3 e 100 caracteres")]
    public string? Cidade { get; set; }

    [Display(Name = "Estado")]
    [Required(ErrorMessage = "O estado é obrigatório")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O estado deve ter entre 3 e 100 caracteres")]
    public string? Estado { get; set; }
}
