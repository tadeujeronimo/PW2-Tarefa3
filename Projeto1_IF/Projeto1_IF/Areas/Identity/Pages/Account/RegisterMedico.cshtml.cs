#nullable disable

using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;

namespace Projeto1_IF.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterMedicoModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly db_IFContext _context;

        public RegisterMedicoModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            db_IFContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public SelectList ListaCidades { get; set; }

        public SelectList ListaPlanos { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Informe o e-mail.")]
            [EmailAddress]
            [Display(Name = "E-mail")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Informe a senha.")]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} e no máximo {1} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Senha")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar senha")]
            [Compare("Password", ErrorMessage = "A senha e a confirmação de senha não conferem.")]
            public string ConfirmPassword { get; set; }

            public ProfissionalRegistroInput Profissional { get; set; } = new();
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            await CarregarListasAsync();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
            {
                await CarregarListasAsync();
                return Page();
            }

            try
            {
                var user = new IdentityUser
                {
                    UserName = Input.Email,
                    Email = Input.Email
                };

                var result = await _userManager.CreateAsync(user, Input.Password);

                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    await CarregarListasAsync();
                    return Page();
                }

                await _userManager.AddToRoleAsync(user, "Medico");

                var contrato = new TbContrato
                {
                    IdPlano = Input.Profissional.IdPlano,
                    DataInicio = DateTime.Now,
                    DataFim = DateTime.Now.AddMonths(1)
                };
                _context.TbContrato.Add(contrato);
                await _context.SaveChangesAsync();

                var profissional = new TbProfissional
                {
                    IdUser = user.Id,
                    IdContrato = contrato.IdContrato,
                    Nome = Input.Profissional.Nome,
                    Cpf = Input.Profissional.Cpf,
                    CrmCrn = Input.Profissional.CrmCrn,
                    Especialidade = Input.Profissional.Especialidade,
                    Logradouro = Input.Profissional.Logradouro,
                    Numero = Input.Profissional.Numero,
                    Bairro = Input.Profissional.Bairro,
                    Cep = Input.Profissional.Cep,
                    IdCidade = Input.Profissional.IdCidade,
                    Ddd1 = Input.Profissional.Ddd1,
                    Telefone1 = Input.Profissional.Telefone1,
                    Ddd2 = Input.Profissional.Ddd2,
                    Telefone2 = Input.Profissional.Telefone2,
                    Salario = Input.Profissional.Salario
                };
                _context.TbProfissional.Add(profissional);
                await _context.SaveChangesAsync();

                await _signInManager.SignInAsync(user, isPersistent: false);
                return LocalRedirect(returnUrl);
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError(string.Empty, "Não foi possível concluir o cadastro. " +
                    "Tente novamente e, se o problema persistir, contate o administrador do sistema.");
                await CarregarListasAsync();
                return Page();
            }
        }

        private async Task CarregarListasAsync()
        {
            ListaCidades = new SelectList(await _context.TbCidade.OrderBy(c => c.Nome).ToListAsync(), "IdCidade", "Nome");

            ListaPlanos = new SelectList(
                await _context.TbPlano
                    .Where(p => p.Nome.Contains("dico"))
                    .OrderBy(p => p.Nome)
                    .ToListAsync(),
                "IdPlano", "Nome");
        }
    }
}
