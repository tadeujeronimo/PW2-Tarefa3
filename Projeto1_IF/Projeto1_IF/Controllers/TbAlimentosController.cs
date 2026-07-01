
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Projeto1_IF.Models;

// Tadeu dos Santos Jerônimo
namespace Projeto1_IF.Controllers
{
    [Authorize]
    public class TbAlimentosController : Controller
    {
        private readonly db_IFContext _context;

        public TbAlimentosController(db_IFContext context)
        {
            _context = context;
        }

        // GET: TBALIMENTOS
        public async Task<IActionResult> Index()
        {
            return View(await _context.TbAlimento.ToListAsync());
        }

        // GET: TBALIMENTOS/Details/5
        public async Task<IActionResult> Details(int? idalimento)
        {
            if (idalimento == null)
            {
                return NotFound();
            }

            var tbalimento = await _context.TbAlimento
                .FirstOrDefaultAsync(m => m.IdAlimento == idalimento);
            if (tbalimento == null)
            {
                return NotFound();
            }

            return View(tbalimento);
        }

        // GET: TBALIMENTOS/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TBALIMENTOS/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdAlimento,IdTipoQuantidade,Nome,Carboidrato,VitaminaA,VitaminaB,TbReceitaAlimentarPadraoXAlimento")] TbAlimento tbalimento)
        {
            if (ModelState.IsValid)
            {
                _context.Add(tbalimento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(tbalimento);
        }

        // GET: TBALIMENTOS/Edit/5
        public async Task<IActionResult> Edit(int? idalimento)
        {
            if (idalimento == null)
            {
                return NotFound();
            }

            var tbalimento = await _context.TbAlimento.FindAsync(idalimento);
            if (tbalimento == null)
            {
                return NotFound();
            }
            return View(tbalimento);
        }

        // POST: TBALIMENTOS/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int? idalimento, [Bind("IdAlimento,IdTipoQuantidade,Nome,Carboidrato,VitaminaA,VitaminaB,TbReceitaAlimentarPadraoXAlimento")] TbAlimento tbalimento)
        {
            if (idalimento != tbalimento.IdAlimento)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(tbalimento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TbAlimentoExists(tbalimento.IdAlimento))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(tbalimento);
        }

        // GET: TBALIMENTOS/Delete/5
        public async Task<IActionResult> Delete(int? idalimento)
        {
            if (idalimento == null)
            {
                return NotFound();
            }

            var tbalimento = await _context.TbAlimento
                .FirstOrDefaultAsync(m => m.IdAlimento == idalimento);
            if (tbalimento == null)
            {
                return NotFound();
            }

            return View(tbalimento);
        }

        // POST: TBALIMENTOS/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? idalimento)
        {
            var tbalimento = await _context.TbAlimento.FindAsync(idalimento);
            if (tbalimento != null)
            {
                _context.TbAlimento.Remove(tbalimento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TbAlimentoExists(int? idalimento)
        {
            return _context.TbAlimento.Any(e => e.IdAlimento == idalimento);
        }
    }
}



