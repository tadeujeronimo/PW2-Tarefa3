// Tadeu dos Santos Jerônimo
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;

namespace Projeto1_IF.Controllers
{
    // Todos os papéis do sistema acessam este controller.
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

        // GerenteGeral acessa qualquer profissional; gerentes de categoria
        // só acessam os da sua categoria; os demais só o próprio cadastro.
        private async Task<bool> PodeAcessarAsync(string idUserDoProfissional)
        {
            if (User.IsInRole("GerenteGeral")) return true;

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

            return idUserDoProfissional == _userManager.GetUserId(User);
        }

        // Retorna true se o usuário logado é algum tipo de gerente.
        private bool EhGerente() =>
            User.IsInRole("GerenteMedico") || User.IsInRole("GerenteNutricionista") || User.IsInRole("GerenteGeral");

        // Gerentes veem a lista; médico/nutricionista vai direto para os próprios detalhes.
        public async Task<IActionResult> Index()
        {
            if (!EhGerente())
            {
                var meuId = _userManager.GetUserId(User);
                var meuProfissional = await _context.TbProfissional
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.IdUser == meuId);

                if (meuProfissional == null) return NotFound();
                return RedirectToAction(nameof(Details), new { idprofissional = meuProfissional.IdProfissional });
            }

            IQueryable<TbProfissional> consulta = _context.TbProfissional
                .Include(p => p.IdCidadeNavigation)
                .Include(p => p.IdContratoNavigation).ThenInclude(c => c.IdPlanoNavigation)
                .AsNoTracking();

            // Filtra por categoria conforme o papel do gerente.
            if (User.IsInRole("GerenteMedico"))
            {
                var ids = (await _userManager.GetUsersInRoleAsync("Medico")).Select(u => u.Id).ToList();
                consulta = consulta.Where(p => ids.Contains(p.IdUser));
            }
            else if (User.IsInRole("GerenteNutricionista"))
            {
                var ids = (await _userManager.GetUsersInRoleAsync("Nutricionista")).Select(u => u.Id).ToList();
                consulta = consulta.Where(p => ids.Contains(p.IdUser));
            }

            return View(await consulta.ToListAsync());
        }

        public async Task<IActionResult> Details(int? idprofissional)
        {
            if (idprofissional == null) return NotFound();

            var tbprofissional = await _context.TbProfissional
                .Include(p => p.IdCidadeNavigation)
                .Include(p => p.IdContratoNavigation).ThenInclude(c => c.IdPlanoNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProfissional == idprofissional);

            if (tbprofissional == null) return NotFound();
            if (!await PodeAcessarAsync(tbprofissional.IdUser)) return Forbid();

            return View(tbprofissional);
        }

        // GET: gerentes podem editar o CPF; o próprio profissional não.
        public async Task<IActionResult> Edit(int? idprofissional)
        {
            if (idprofissional == null) return NotFound();

            var tbprofissional = await _context.TbProfissional.FindAsync(idprofissional);
            if (tbprofissional == null) return NotFound();
            if (!await PodeAcessarAsync(tbprofissional.IdUser)) return Forbid();

            ViewData["PodeEditarCpf"] = EhGerente();
            await CarregarViewDataAsync(tbprofissional);
            return View(tbprofissional);
        }

        // POST: gerentes têm CPF na lista de campos; profissional comum não.
        // Renomeado para EditPost com [ActionName] para evitar conflito com o GET.
        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? idprofissional)
        {
            if (idprofissional == null) return NotFound();

            var tbprofissionalToUpdate = await _context.TbProfissional
                .FirstOrDefaultAsync(p => p.IdProfissional == idprofissional);

            if (tbprofissionalToUpdate == null) return NotFound();
            if (!await PodeAcessarAsync(tbprofissionalToUpdate.IdUser)) return Forbid();

            bool ehGerente = EhGerente();
            ViewData["PodeEditarCpf"] = ehGerente;

            bool atualizou = ehGerente
                ? await TryUpdateModelAsync(tbprofissionalToUpdate, "",
                    p => p.Nome, p => p.Cpf, p => p.CrmCrn, p => p.Especialidade, p => p.Logradouro,
                    p => p.Numero, p => p.Bairro, p => p.Cep, p => p.IdCidade, p => p.Ddd1,
                    p => p.Telefone1, p => p.Ddd2, p => p.Telefone2, p => p.Salario, p => p.IdTipoAcesso)
                : await TryUpdateModelAsync(tbprofissionalToUpdate, "",
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
                    if (!TbProfissionalExists(tbprofissionalToUpdate.IdProfissional)) return NotFound();
                    else throw;
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Não foi possível salvar. Tente novamente.");
                }
            }

            await CarregarViewDataAsync(tbprofissionalToUpdate);
            return View(tbprofissionalToUpdate);
        }

        // Exclusão restrita a gerentes; bloqueada se o profissional tiver pacientes.
        public async Task<IActionResult> Delete(int? idprofissional, bool? saveChangesError = false)
        {
            if (!EhGerente()) return Forbid();
            if (idprofissional == null) return NotFound();

            var tbprofissional = await _context.TbProfissional
                .Include(p => p.IdCidadeNavigation)
                .Include(p => p.IdContratoNavigation).ThenInclude(c => c.IdPlanoNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdProfissional == idprofissional);

            if (tbprofissional == null) return NotFound();
            if (!await PodeAcessarAsync(tbprofissional.IdUser)) return Forbid();

            ViewData["PossuiPacientes"] = await _context.TbMedicoPaciente
                .AnyAsync(m => m.IdProfissional == idprofissional);

            if (saveChangesError.GetValueOrDefault())
                ViewData["ErrorMessage"] = "A exclusão falhou. Tente novamente.";

            return View(tbprofissional);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? idprofissional)
        {
            if (!EhGerente()) return Forbid();

            var tbprofissional = await _context.TbProfissional.FindAsync(idprofissional);
            if (tbprofissional == null) return RedirectToAction(nameof(Index));
            if (!await PodeAcessarAsync(tbprofissional.IdUser)) return Forbid();

            // Não permite excluir se houver pacientes vinculados.
            if (await _context.TbMedicoPaciente.AnyAsync(m => m.IdProfissional == idprofissional))
                return RedirectToAction(nameof(Delete), new { idprofissional, saveChangesError = true });

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

        // Popula os dropdowns de cidade e tipo de acesso.
        private async Task CarregarViewDataAsync(TbProfissional tbprofissional)
        {
            ViewData["IdCidade"] = new SelectList(_context.TbCidade.OrderBy(c => c.Nome), "IdCidade", "Nome", tbprofissional.IdCidade);
            ViewData["IdTipoAcesso"] = new SelectList(await _context.TbTipoAcesso.OrderBy(t => t.Nome).ToListAsync(), "IdTipoAcesso", "Nome", tbprofissional.IdTipoAcesso);
        }

        private bool TbProfissionalExists(int idprofissional) =>
            _context.TbProfissional.Any(e => e.IdProfissional == idprofissional);
    }
}
