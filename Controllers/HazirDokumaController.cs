using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class HazirDokumaController : Controller
{
    private readonly ApplicationDbContext _context;

    public HazirDokumaController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: HazirDokuma Listesi
    public async Task<IActionResult> Index()
    {
        var hazirDokumaList = await _context.HazirDokuma
            .Where(x => !x.Tamamlandi)
            .OrderByDescending(x => x.KayitTarihi)
            .ToListAsync();
        return View(hazirDokumaList);
    }

    // GET: Yeni Kayıt Formu
    public IActionResult Create()
    {
        return View();
    }

    // POST: Yeni Kayıt Ekleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HazirDokuma hazirDokuma)
    {
        if (ModelState.IsValid)
        {
            // Computed column'ları NULL yap
            hazirDokuma.GecenSure = null;

            _context.Add(hazirDokuma);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Hazır Dokuma kaydı başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(hazirDokuma);
    }

    // POST: Tamamlama İşlemi
    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        var hazirDokuma = await _context.HazirDokuma.FindAsync(id);
        if (hazirDokuma != null)
        {
            hazirDokuma.Tamamlandi = true;
            hazirDokuma.BitisTarihi = DateTime.Now;

            // GecenSure computed column - güncelleme YOK
            await _context.SaveChangesAsync();
            TempData["Success"] = "Hazır Dokuma işlemi tamamlandı!";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Silme İşlemi
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var hazirDokuma = await _context.HazirDokuma.FindAsync(id);
        if (hazirDokuma != null)
        {
            _context.HazirDokuma.Remove(hazirDokuma);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Hazır Dokuma kaydı silindi!";
        }
        return RedirectToAction(nameof(Index));
    }
}