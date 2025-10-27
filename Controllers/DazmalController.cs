using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class DazmalController : Controller
{
    private readonly ApplicationDbContext _context;

    public DazmalController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Dazmal Listesi
    public async Task<IActionResult> Index()
    {
        var dazmalList = await _context.Dazmal
            .Where(x => !x.Tamamlandi)
            .OrderByDescending(x => x.KayitTarihi)
            .ToListAsync();
        return View(dazmalList);
    }

    // GET: Yeni Kayıt Formu
    public IActionResult Create()
    {
        return View();
    }

    // POST: Yeni Kayıt Ekleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Dazmal dazmal)
    {
        if (ModelState.IsValid)
        {
            _context.Add(dazmal);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Dazmal kaydı başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(dazmal);
    }

    // POST: Ekleme'den Otomatik Geçiş
    [HttpPost]
    public async Task<IActionResult> AddFromEkleme(int eklemeId)
    {
        var ekleme = await _context.Ekleme.FindAsync(eklemeId);
        if (ekleme != null)
        {
            // Dazmal kaydı oluştur
            var dazmal = new Dazmal
            {
                SiparisNo = ekleme.SiparisNo,
                UrunAdi = ekleme.UrunAdi,
                BaslamaTarihi = DateTime.Now, // Ekleme bitiş tarihi başlangıç olacak
                Aciklama = $"Ekleme'den otomatik geçiş - {ekleme.Aciklama}",
                Tamamlandi = false,
                KayitTarihi = DateTime.Now
            };

            _context.Dazmal.Add(dazmal);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{ekleme.SiparisNo} numaralı sipariş Dazmal'a taşındı!";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Completed İşlemi
    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        var dazmal = await _context.Dazmal.FindAsync(id);
        if (dazmal != null)
        {
            dazmal.Tamamlandi = true;
            dazmal.BitisTarihi = DateTime.Now;

            await _context.SaveChangesAsync();
            TempData["Success"] = "Dazmal işlemi tamamlandı!";
        }
        return RedirectToAction(nameof(Index));
    }

    // Geçen süreyi hesapla (saat cinsinden)
    private string CalculateTimeDifference(DateTime? baslangic, DateTime bitis)
    {
        if (!baslangic.HasValue) return "0 saat";

        var fark = bitis - baslangic.Value;
        var toplamSaat = (int)fark.TotalHours;
        var dakika = fark.Minutes;

        return $"{toplamSaat} saat {dakika} dakika";
    }

    // POST: Silme İşlemi
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var dazmal = await _context.Dazmal.FindAsync(id);
        if (dazmal != null)
        {
            _context.Dazmal.Remove(dazmal);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Dazmal kaydı silindi!";
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Temizleme (Tüm tamamlananları sil)
    public async Task<IActionResult> Clear()
    {
        var tamamlananlar = await _context.Dazmal.Where(x => x.Tamamlandi).ToListAsync();
        _context.Dazmal.RemoveRange(tamamlananlar);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"{tamamlananlar.Count} adet tamamlanmış kayıt silindi!";
        return RedirectToAction(nameof(Index));
    }
}