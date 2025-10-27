using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class KesmeController : Controller
{
    private readonly ApplicationDbContext _context;

    public KesmeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Kesme Listesi
    public async Task<IActionResult> Index()
    {
        var kesmeList = await _context.Kesme
            .Where(x => !x.Tamamlandi)
            .OrderByDescending(x => x.KayitTarihi)
            .ToListAsync();
        return View(kesmeList);
    }

    // GET: Yeni Kayıt Formu
    public IActionResult Create()
    {
        return View();
    }

    // POST: Yeni Kayıt Ekleme - GECENSURE OLMADAN
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Kesme kesme)
    {
        if (ModelState.IsValid)
        {
            // GecenSure'ü NULL yap (computed column)
            kesme.GecenSure = null;

            _context.Add(kesme);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Kesme kaydı başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(kesme);
    }

    // POST: Tamamlama İşlemi - GECENSURE GÜNCELLEME YOK
    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        var kesme = await _context.Kesme.FindAsync(id);
        if (kesme != null)
        {
            kesme.Tamamlandi = true;
            kesme.BitisTarihi = DateTime.Now;

            // GECENSURE GÜNCELLEME YOK - computed column olduğu için
            await _context.SaveChangesAsync();
            TempData["Success"] = "Kesme işlemi tamamlandı!";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Silme İşlemi
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var kesme = await _context.Kesme.FindAsync(id);
        if (kesme != null)
        {
            _context.Kesme.Remove(kesme);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Kesme kaydı silindi!";
        }
        return RedirectToAction(nameof(Index));
    }
}