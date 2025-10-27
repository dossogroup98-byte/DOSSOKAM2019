using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class DokumaciController : Controller
{
    private readonly ApplicationDbContext _context;

    public DokumaciController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Dokumaci
    public async Task<IActionResult> Index()
    {
        var dokumacilar = await _context.Dokumacilar.ToListAsync();
        return View(dokumacilar);
    }

    // GET: Dokumaci/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Dokumaci/Create
    [HttpPost]
    public async Task<IActionResult> Create(Dokumaci dokumaci)
    {
        if (ModelState.IsValid)
        {
            dokumaci.KayitTarihi = DateTime.Now;
            _context.Add(dokumaci);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(dokumaci);
    }

    // GET: Dokumaci/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var dokumaci = await _context.Dokumacilar.FindAsync(id);
        if (dokumaci == null) return NotFound();
        return View(dokumaci);
    }

    // POST: Dokumaci/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Dokumaci dokumaci)
    {
        if (id != dokumaci.DokumaciID) return NotFound();

        if (ModelState.IsValid)
        {
            _context.Update(dokumaci);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(dokumaci);
    }

    // GET: Dokumaci/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();
        var dokumaci = await _context.Dokumacilar.FindAsync(id);
        if (dokumaci == null) return NotFound();
        return View(dokumaci);
    }

    // POST: Dokumaci/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var dokumaci = await _context.Dokumacilar.FindAsync(id);
        _context.Dokumacilar.Remove(dokumaci);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}