// Tadeu dos Santos Jerônimo
// ViewModel usado somente no autocadastro (RegisterMedico/RegisterNutricionista).
// Contém apenas os campos que o próprio usuário deve poder preencher.
// IdProfissional, IdUser e IdContrato NUNCA aparecem aqui: são definidos
// pelo código no servidor, nunca vindos do formulário (evita overposting).
using System.ComponentModel.DataAnnotations;

namespace Projeto1_IF.Models;

public class ProfissionalRegistroInput
{
    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(100)]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "Informe o CPF.")]
    [StringLength(15)]
    [Display(Name = "CPF")]
    public string Cpf { get; set; }

    [StringLength(20)]
    [Display(Name = "CRM/CRN")]
    public string CrmCrn { get; set; }

    [StringLength(100)]
    [Display(Name = "Especialidade")]
    public string Especialidade { get; set; }

    [StringLength(100)]
    [Display(Name = "Logradouro")]
    public string Logradouro { get; set; }

    [Required(ErrorMessage = "Informe o número.")]
    [StringLength(10)]
    [Display(Name = "Número")]
    public string Numero { get; set; }

    [Required(ErrorMessage = "Informe o bairro.")]
    [StringLength(100)]
    [Display(Name = "Bairro")]
    public string Bairro { get; set; }

    [Required(ErrorMessage = "Informe o CEP.")]
    [StringLength(10)]
    [Display(Name = "CEP")]
    public string Cep { get; set; }

    [Required(ErrorMessage = "Selecione a cidade.")]
    [Display(Name = "Cidade")]
    public int IdCidade { get; set; }

    [StringLength(2)]
    [Display(Name = "DDD")]
    public string Ddd1 { get; set; }

    [StringLength(25)]
    [Display(Name = "Telefone")]
    public string Telefone1 { get; set; }

    [StringLength(2)]
    [Display(Name = "DDD (2)")]
    public string Ddd2 { get; set; }

    [StringLength(25)]
    [Display(Name = "Telefone (2)")]
    public string Telefone2 { get; set; }

    [Display(Name = "Salário")]
    public decimal? Salario { get; set; }

    [Required(ErrorMessage = "Selecione o plano.")]
    [Display(Name = "Plano")]
    public int IdPlano { get; set; }
}
