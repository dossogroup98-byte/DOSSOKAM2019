using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class PaketlemeController : Controller
{
    private readonly ApplicationDbContext _context;

    public PaketlemeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Paketleme Listesi
    public async Task<IActionResult> Index()
    {
        var paketlemeList = await _context.Paketleme
            .Where(x => !x.Tamamlandi)
            .OrderByDescending(x => x.KayitTarihi)
            .ToListAsync();
        return View(paketlemeList);
    }

    // GET: Yeni Kayıt Formu
    public IActionResult Create()
    {
        return View();
    }

    // POST: Yeni Kayıt Ekleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Paketleme paketleme)
    {
        if (ModelState.IsValid)
        {
            _context.Add(paketleme);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Paketleme kaydı başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(paketleme);
    }

    // POST: Tamamlama İşlemi
    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        var paketleme = await _context.Paketleme.FindAsync(id);
        if (paketleme != null)
        {
            paketleme.Tamamlandi = true;
            paketleme.BitisTarihi = DateTime.Now;

            // Süreyi hesapla
            if (paketleme.BaslamaTarihi.HasValue)
            {
                var gecenSure = CalculateTimeDifferenceInHours(paketleme.BaslamaTarihi.Value, DateTime.Now);
                paketleme.GecenSure = (paketleme.GecenSure ?? 0) + gecenSure;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Paketleme işlemi tamamlandı! Toplam süre: {paketleme.GecenSure:F2} saat";
        }
        return RedirectToAction(nameof(Index));
    }

    private decimal CalculateTimeDifferenceInHours(DateTime baslangic, DateTime bitis)
    {
        var fark = bitis - baslangic;
        return (decimal)fark.TotalHours;
    }

    // POST: Silme İşlemi
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var paketleme = await _context.Paketleme.FindAsync(id);
        if (paketleme != null)
        {
            _context.Paketleme.Remove(paketleme);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Paketleme kaydı silindi!";
        }
        return RedirectToAction(nameof(Index));
    }
}