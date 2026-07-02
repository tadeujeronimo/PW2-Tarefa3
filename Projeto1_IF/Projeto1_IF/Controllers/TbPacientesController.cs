// Tadeu dos Santos Jerônimo
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;

namespace Projeto1_IF.Controllers
{
    // Apenas Médico e Nutricionista acessam esta área.
    [Authorize(Roles = "Medico,Nutricionista")]
    public class TbPacientesController : Controller
    {
        private readonly db_IFContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TbPacientesController(db_IFContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Busca o IdProfissional do usuário logado.
        private async Task<int?> ObterIdProfissionalLogadoAsync()
        {
            var userId = _userManager.GetUserId(User);
            return await _context.TbProfissional
                .Where(p => p.IdUser == userId)
                .Select(p => (int?)p.IdProfissional)
                .FirstOrDefaultAsync();
        }

        // Verifica se o paciente pertence ao profissional logado.
        private async Task<bool> PacienteEhDoProfissionalLogadoAsync(int idpaciente)
        {
            var meuId = await ObterIdProfissionalLogadoAsync();
            if (meuId == null) return false;
            return await _context.TbMedicoPaciente
                .AnyAsync(m => m.IdPaciente == idpaciente && m.IdProfissional == meuId);
        }

        // Lista só os pacientes do profissional logado.
        public async Task<IActionResult> Index()
        {
            var meuId = await ObterIdProfissionalLogadoAsync();
            if (meuId == null) return Forbid();

            var pacientes = await _context.TbPaciente
                .Include(p => p.IdCidadeNavigation)
                .Where(p => p.TbMedicoPaciente.Any(m => m.IdProfissional == meuId))
                .AsNoTracking()
                .ToListAsync();

            return View("~/Views/TbPaciente/Index.cshtml", pacientes);
        }

        public async Task<IActionResult> Details(int? idpaciente)
        {
            if (idpaciente == null) return NotFound();
            if (!await PacienteEhDoProfissionalLogadoAsync(idpaciente.Value)) return Forbid();

            var tbpaciente = await _context.TbPaciente
                .Include(p => p.IdCidadeNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdPaciente == idpaciente);

            if (tbpaciente == null) return NotFound();
            return View("~/Views/TbPaciente/Details.cshtml", tbpaciente);
        }

        // GET: carrega o dropdown de cidades para o formulário.
        public IActionResult Create()
        {
            ViewData["IdCidade"] = new SelectList(_context.TbCidade.OrderBy(c => c.Nome), "IdCidade", "Nome");
            return View("~/Views/TbPaciente/Create.cshtml");
        }

        // POST: IdPaciente fica fora do Bind — é gerado pelo banco.
        // Após salvar, cria o vínculo do paciente com o profissional.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nome,Rg,Cpf,DataNascimento,NomeResponsavel,Sexo,Etnia,Endereco,Bairro,IdCidade,TelResidencial,TelComercial,TelCelular,Profissao,FlgAtleta,FlgGestante")] TbPaciente tbpaciente)
        {
            var meuId = await ObterIdProfissionalLogadoAsync();
            if (meuId == null) return Forbid();

            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(tbpaciente);
                    await _context.SaveChangesAsync();

                    _context.TbMedicoPaciente.Add(new TbMedicoPaciente
                    {
                        IdPaciente = tbpaciente.IdPaciente,
                        IdProfissional = meuId.Value
                    });
                    await _context.SaveChangesAsync();

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Não foi possível salvar. Tente novamente.");
            }

            ViewData["IdCidade"] = new SelectList(_context.TbCidade.OrderBy(c => c.Nome), "IdCidade", "Nome", tbpaciente.IdCidade);
            return View("~/Views/TbPaciente/Create.cshtml", tbpaciente);
        }

        // GET: carrega o formulário com a cidade já selecionada.
        public async Task<IActionResult> Edit(int? idpaciente)
        {
            if (idpaciente == null) return NotFound();
            if (!await PacienteEhDoProfissionalLogadoAsync(idpaciente.Value)) return Forbid();

            var tbpaciente = await _context.TbPaciente.FindAsync(idpaciente);
            if (tbpaciente == null) return NotFound();

            ViewData["IdCidade"] = new SelectList(_context.TbCidade.OrderBy(c => c.Nome), "IdCidade", "Nome", tbpaciente.IdCidade);
            return View("~/Views/TbPaciente/Edit.cshtml", tbpaciente);
        }

        // POST: TryUpdateModelAsync atualiza só os campos listados,
        // mantendo os demais exatamente como estão no banco.
        // Renomeado para EditPost com [ActionName] para evitar conflito
        // de assinatura com o método GET acima.
        [HttpPost]
        [ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? idpaciente)
        {
            if (idpaciente == null) return NotFound();
            if (!await PacienteEhDoProfissionalLogadoAsync(idpaciente.Value)) return Forbid();

            var tbpacienteToUpdate = await _context.TbPaciente.FirstOrDefaultAsync(p => p.IdPaciente == idpaciente);
            if (tbpacienteToUpdate == null) return NotFound();

            if (await TryUpdateModelAsync(
                tbpacienteToUpdate, "",
                p => p.Nome, p => p.Rg, p => p.Cpf, p => p.DataNascimento, p => p.NomeResponsavel,
                p => p.Sexo, p => p.Etnia, p => p.Endereco, p => p.Bairro, p => p.IdCidade,
                p => p.TelResidencial, p => p.TelComercial, p => p.TelCelular, p => p.Profissao,
                p => p.FlgAtleta, p => p.FlgGestante))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TbPacienteExists(tbpacienteToUpdate.IdPaciente)) return NotFound();
                    else throw;
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Não foi possível salvar. Tente novamente.");
                }
            }

            ViewData["IdCidade"] = new SelectList(_context.TbCidade.OrderBy(c => c.Nome), "IdCidade", "Nome", tbpacienteToUpdate.IdCidade);
            return View("~/Views/TbPaciente/Edit.cshtml", tbpacienteToUpdate);
        }

        // GET: se a exclusão anterior falhou, exibe a mensagem de erro.
        public async Task<IActionResult> Delete(int? idpaciente, bool? saveChangesError = false)
        {
            if (idpaciente == null) return NotFound();
            if (!await PacienteEhDoProfissionalLogadoAsync(idpaciente.Value)) return Forbid();

            var tbpaciente = await _context.TbPaciente
                .Include(p => p.IdCidadeNavigation)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.IdPaciente == idpaciente);

            if (tbpaciente == null) return NotFound();

            if (saveChangesError.GetValueOrDefault())
                ViewData["ErrorMessage"] = "A exclusão falhou. Tente novamente.";

            return View("~/Views/TbPaciente/Delete.cshtml", tbpaciente);
        }

        // POST: remove o vínculo antes de excluir o paciente.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? idpaciente)
        {
            if (idpaciente == null || !await PacienteEhDoProfissionalLogadoAsync(idpaciente.Value))
                return Forbid();

            var tbpaciente = await _context.TbPaciente.FindAsync(idpaciente);
            if (tbpaciente == null) return RedirectToAction(nameof(Index));

            try
            {
                var vinculo = await _context.TbMedicoPaciente
                    .FirstOrDefaultAsync(m => m.IdPaciente == idpaciente);
                if (vinculo != null) _context.TbMedicoPaciente.Remove(vinculo);

                _context.TbPaciente.Remove(tbpaciente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                return RedirectToAction(nameof(Delete), new { idpaciente, saveChangesError = true });
            }
        }

        private bool TbPacienteExists(int? idpaciente) =>
            _context.TbPaciente.Any(e => e.IdPaciente == idpaciente);
    }
}
