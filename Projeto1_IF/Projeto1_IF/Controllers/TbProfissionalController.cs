// Tadeu dos Santos Jerônimo
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;

namespace Projeto1_IF.Controllers
{
    // Só usuários autenticados com algum desses papéis entram aqui.
    // - Medico / Nutricionista: só enxergam e editam o PRÓPRIO cadastro.
    // - GerenteMedico / GerenteNutricionista: editam, veem e excluem os
    //   profissionais da sua categoria (mas não criam: criação só pelo
    //   autocadastro em RegisterMedico/RegisterNutricionista).
    // - GerenteGeral: acesso completo (exceto Create, igual aos outros).
    [Authorize(Roles = "Medico,Nutricionista,GerenteMedico,GerenteNutricionista,GerenteGeral")]
    public class TbProfissionalController : Controller
    {
        private readonly db_IFContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TbProfissionalController(db_IFContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Determina se o usuário logado pode acessar (ver/editar) o
        // profissional dono do IdUser informado.
        private async Task<bool> PodeAcessarAsync(string idUserDoProfissional)
        {
            if (User.IsInRole("GerenteGeral"))
            {
                return true;
            }

            if (User.IsInRole("GerenteMedico"))
            {
                var dono = await _userManager.FindByIdAsync(idUserDoProfissional);
                return dono != null && await _userManager.IsInRoleAsync(dono, "Medico");
            }

            if (User.IsInRole("GerenteNutricionista"))
            {
                var dono = await _userManager.FindByIdAsync(idUserDoProfissional);
                return dono != null && await _userManager.IsInRoleAsync(dono, "Nutricionista");
            }

            // Médico ou Nutricionista comum: só o próprio cadastro.
            return idUserDoProfissional == _userManager.GetUserId(User);
        }

        private bool EhGerente()
        {
            return User.IsInRole("GerenteMedico") || User.IsInRole("GerenteNutricionista") || User.IsInRole("GerenteGeral");
        }

        // GET: TbProfissional
        public async Task<IActionResult> Index()
        {
            // Médico/Nutricionista não têm uma "lista": só o próprio
            // cadastro existe pra eles. Manda direto pros detalhes.
            if (!EhGerente())
            {
                var meuId = _userManager.GetUserId(User);
                var meuProfissional = await _context.TbProfissional
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.IdUser == meuId);

                if (meuProfissional == null)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Details), new { idprofissional = meuProfissional.IdProfissional });
            }

            IQueryable<TbProfissional> consulta = _context.TbProfissional
                .Include(p => p.IdCidadeNavigation)
                .Include(p => p.IdContratoNavigation).ThenInclude(c => c.IdPlanoNavigation)
                .AsNoTracking();

            if (User.IsInRole("GerenteMedico"))
            {
                var idsMedicos = (await _userManager.GetUsersInRoleAsync("Medico")).Select(u => u.Id).ToList();
                consulta = consulta.Where(p => idsMedicos.Contains(p.IdUser));
            }
            else if (User.IsInRole("GerenteNutricionista"))
            {
                var idsNutricionistas = (await _userManager.GetUsersInRoleAsync("Nutricionista")).Select(u => u.Id).ToList();
                consulta = consulta.Where(p => idsNutricionistas.Contains(p.IdUser));
            }
            // GerenteGeral: sem filtro adicional, vê todos.

            return View(await consulta.ToListAsync());
        }

        // GET: TbProfissional/Details/5
        public async Task<IActionResult> Details(int? idprofissional)
        {
            if (idprofissional == null)
            {
                return NotFound();
            }

            var tbprofissional = await _context.TbProfissional
                .Include(p => p.IdCidadeNavigation)
                .Include(p => p.IdContratoNavigation).ThenInclude(c => c.IdPlanoNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProfissional == idprofissional);

            if (tbprofissional == null)
            {
                return NotFound();
            }

            if (!await PodeAcessarAsync(tbprofissional.IdUser))
            {
                return Forbid();
            }

            return View(tbprofissional);
        }

        // GET: TbProfissional/Edit/5
        public async Task<IActionResult> Edit(int? idprofissional)
        {
            if (idprofissional == null)
            {
                return NotFound();
            }

            var tbprofissional = await _context.TbProfissional.FindAsync(idprofissional);
            if (tbprofissional == null)
            {
                return NotFound();
            }

            if (!await PodeAcessarAsync(tbprofissional.IdUser))
            {
                return Forbid();
            }

            // O próprio profissional não pode mudar o CPF; o gerente pode.
            ViewData["PodeEditarCpf"] = EhGerente();
            await CarregarViewDataAsync(tbprofissional);
            return View(tbprofissional);
        }

        // POST: TbProfissional/Edit/5
        // Em vez de [Bind], usamos TryUpdateModelAsync com a lista exata de
        // campos permitidos. Quando NÃO é gerente (ou seja, é o próprio
        // profissional editando o próprio cadastro), o campo Cpf fica de
        // fora dessa lista — então mesmo que alguém manipule o formulário e
        // envie um Cpf diferente, ele é ignorado.
        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? idprofissional)
        {
            if (idprofissional == null)
            {
                return NotFound();
            }

            var tbprofissionalToUpdate = await _context.TbProfissional.FirstOrDefaultAsync(p => p.IdProfissional == idprofissional);
            if (tbprofissionalToUpdate == null)
            {
                return NotFound();
            }

            if (!await PodeAcessarAsync(tbprofissionalToUpdate.IdUser))
            {
                return Forbid();
            }

            bool ehGerente = EhGerente();
            ViewData["PodeEditarCpf"] = ehGerente;

            bool atualizou = ehGerente
                ? await TryUpdateModelAsync(
                    tbprofissionalToUpdate,
                    "",
                    p => p.Nome, p => p.Cpf, p => p.CrmCrn, p => p.Especialidade, p => p.Logradouro,
                    p => p.Numero, p => p.Bairro, p => p.Cep, p => p.IdCidade, p => p.Ddd1,
                    p => p.Telefone1, p => p.Ddd2, p => p.Telefone2, p => p.Salario, p => p.IdTipoAcesso)
                : await TryUpdateModelAsync(
                    tbprofissionalToUpdate,
                    "",
                    p => p.Nome, p => p.CrmCrn, p => p.Especialidade, p => p.Logradouro,
                    p => p.Numero, p => p.Bairro, p => p.Cep, p => p.IdCidade, p => p.Ddd1,
                    p => p.Telefone1, p => p.Ddd2, p => p.Telefone2, p => p.Salario);

            if (atualizou)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return ehGerente
                        ? RedirectToAction(nameof(Index))
                        : RedirectToAction(nameof(Details), new { idprofissional = tbprofissionalToUpdate.IdProfissional });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TbProfissionalExists(tbprofissionalToUpdate.IdProfissional))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Não foi possível salvar as alterações. " +
                        "Tente novamente e, se o problema persistir, contate o administrador do sistema.");
                }
            }

            await CarregarViewDataAsync(tbprofissionalToUpdate);
            return View(tbprofissionalToUpdate);
        }

        // GET: TbProfissional/Delete/5
        // Somente gerentes podem excluir.
        public async Task<IActionResult> Delete(int? idprofissional, bool? saveChangesError = false)
        {
            if (!EhGerente())
            {
                return Forbid();
            }

            if (idprofissional == null)
            {
                return NotFound();
            }

            var tbprofissional = await _context.TbProfissional
                .Include(p => p.IdCidadeNavigation)
                .Include(p => p.IdContratoNavigation).ThenInclude(c => c.IdPlanoNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProfissional == idprofissional);

            if (tbprofissional == null)
            {
                return NotFound();
            }

            if (!await PodeAcessarAsync(tbprofissional.IdUser))
            {
                return Forbid();
            }

            bool possuiPacientes = await _context.TbMedicoPaciente.AnyAsync(m => m.IdProfissional == idprofissional);
            ViewData["PossuiPacientes"] = possuiPacientes;

            if (saveChangesError.GetValueOrDefault())
            {
                ViewData["ErrorMessage"] = "A exclusão falhou. Tente novamente e, se o problema persistir, contate o administrador do sistema.";
            }

            return View(tbprofissional);
        }

        // POST: TbProfissional/Delete/5
        // Só é permitido excluir profissionais que não possuem pacientes
        // cadastrados (regra do enunciado do trabalho final).
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? idprofissional)
        {
            if (!EhGerente())
            {
                return Forbid();
            }

            var tbprofissional = await _context.TbProfissional.FindAsync(idprofissional);
            if (tbprofissional == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!await PodeAcessarAsync(tbprofissional.IdUser))
            {
                return Forbid();
            }

            bool possuiPacientes = await _context.TbMedicoPaciente.AnyAsync(m => m.IdProfissional == idprofissional);
            if (possuiPacientes)
            {
                ModelState.AddModelError("", "Este profissional possui pacientes cadastrados e não pode ser excluído.");
                return RedirectToAction(nameof(Delete), new { idprofissional, saveChangesError = true });
            }

            try
            {
                _context.TbProfissional.Remove(tbprofissional);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                return RedirectToAction(nameof(Delete), new { idprofissional, saveChangesError = true });
            }
        }

        private async Task CarregarViewDataAsync(TbProfissional tbprofissional)
        {
            ViewData["IdCidade"] = new SelectList(_context.TbCidade.OrderBy(c => c.Nome), "IdCidade", "Nome", tbprofissional.IdCidade);
            ViewData["IdTipoAcesso"] = new SelectList(await _context.TbTipoAcesso.OrderBy(t => t.Nome).ToListAsync(), "IdTipoAcesso", "Nome", tbprofissional.IdTipoAcesso);
        }

        private bool TbProfissionalExists(int idprofissional)
        {
            return _context.TbProfissional.Any(e => e.IdProfissional == idprofissional);
        }
    }
}
