using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class EklemeController : Controller
{
    private readonly ApplicationDbContext _context;

    public EklemeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Ekleme Listesi
    public async Task<IActionResult> Index()
    {
        var eklemeList = await _context.Ekleme
            .Where(x => !x.Tamamlandi)
            .OrderByDescending(x => x.KayitTarihi)
            .ToListAsync();
        return View(eklemeList);
    }

    // GET: Yeni Kayıt Formu
    public IActionResult Create()
    {
        return View();
    }

    // POST: Yeni Kayıt Ekleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ekleme ekleme)
    {
        if (ModelState.IsValid)
        {
            _context.Add(ekleme);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Ekleme kaydı başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(ekleme);
    }

    // POST: Makine Saat'ten Otomatik Geçiş
    [HttpPost]
    public async Task<IActionResult> AddFromMakineSaat(int makineSaatId)
    {
        var makineSaat = await _context.MakineSaat.FindAsync(makineSaatId);
        if (makineSaat != null)
        {
            // Ekleme kaydı oluştur
            var ekleme = new Ekleme
            {
                SiparisNo = makineSaat.SiparisNo,
                UrunAdi = makineSaat.UrunAdi,
                BaslamaTarihi = DateTime.Now, // Makine Saat bitiş tarihi başlangıç olacak
                Aciklama = $"Makine Saat'ten otomatik geçiş - {makineSaat.Aciklama}",
                Tamamlandi = false,
                KayitTarihi = DateTime.Now
            };

            _context.Ekleme.Add(ekleme);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{makineSaat.SiparisNo} numaralı sipariş Ekleme'ye taşındı!";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Completed İşlemi
    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        var ekleme = await _context.Ekleme.FindAsync(id);
        if (ekleme != null)
        {
            ekleme.Tamamlandi = true;
            ekleme.BitisTarihi = DateTime.Now;

            // OTOMATİK DAZMAL'A GEÇİŞ
            var dazmal = new Dazmal
            {
                SiparisNo = ekleme.SiparisNo,
                UrunAdi = ekleme.UrunAdi,
                BaslamaTarihi = DateTime.Now, // Ekleme bitişi Dazmal başlangıcı
                Aciklama = $"Ekleme tamamlandı", // SADELEŞTİRİLMİŞ AÇIKLAMA
                Tamamlandi = false,
                KayitTarihi = DateTime.Now
            };

            _context.Dazmal.Add(dazmal);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{ekleme.SiparisNo} tamamlandı ve Dazmal'a taşındı!";
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
        var ekleme = await _context.Ekleme.FindAsync(id);
        if (ekleme != null)
        {
            _context.Ekleme.Remove(ekleme);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Ekleme kaydı silindi!";
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: Temizleme (Tüm tamamlananları sil)
    public async Task<IActionResult> Clear()
    {
        var tamamlananlar = await _context.Ekleme.Where(x => x.Tamamlandi).ToListAsync();
        _context.Ekleme.RemoveRange(tamamlananlar);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"{tamamlananlar.Count} adet tamamlanmış kayıt silindi!";
        return RedirectToAction(nameof(Index));
    }
}