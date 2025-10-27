using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class MakineSaatController : Controller
{
    private readonly ApplicationDbContext _context;

    public MakineSaatController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: MakineSaat Listesi
    public async Task<IActionResult> Index()
    {
        var makineSaatList = await _context.MakineSaat
            .OrderByDescending(x => x.KayitTarihi)
            .ToListAsync();
        return View(makineSaatList);
    }

    // GET: Yeni Kayıt Formu
    public IActionResult Create()
    {
        // ✅ DOKUMACILARI VIEWBAG'E EKLE
        ViewBag.Dokumacilar = _context.Dokumacilar.Where(d => d.Aktif).ToList();
        return View();
    }

    // POST: Yeni Kayıt Ekleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MakineSaat makineSaat)
    {
        if (ModelState.IsValid)
        {
            makineSaat.KayitTarihi = DateTime.Now;
            _context.Add(makineSaat);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Makine Saat kaydı başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }

        // ✅ FORM HATALIYSA DOKUMACILARI TEKRAR YÜKLE
        ViewBag.Dokumacilar = _context.Dokumacilar.Where(d => d.Aktif).ToList();
        return View(makineSaat);
    }

    // POST: Tamamlama İşlemi - EKLEME'YE TRANSFER
    [HttpPost]
    public async Task<IActionResult> Complete(int id)
    {
        try
        {
            var makineSaat = await _context.MakineSaat.FindAsync(id);
            if (makineSaat == null)
            {
                TempData["Error"] = "Kayıt bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            // 1. Ekleme'ye yeni kayıt oluştur
            var ekleme = new Ekleme
            {
                SiparisNo = makineSaat.SiparisNo,
                UrunAdi = makineSaat.UrunAdi,
                BaslamaTarihi = DateTime.Now,
                Aciklama = $"Makine Saat'ten otomatik geçiş - Kalan Saat: {makineSaat.KalanSaat}",
                Tamamlandi = false,
                KayitTarihi = DateTime.Now
            };

            _context.Ekleme.Add(ekleme);

            // 2. MakineSaat'ten KAYDI SİL
            _context.MakineSaat.Remove(makineSaat);

            // 3. Her ikisini de kaydet
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{makineSaat.SiparisNo} Ekleme'ye taşındı ve listeden kaldırıldı!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Hata: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: Silme İşlemi
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var makineSaat = await _context.MakineSaat.FindAsync(id);
        if (makineSaat != null)
        {
            _context.MakineSaat.Remove(makineSaat);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Makine Saat kaydı silindi!";
        }
        return RedirectToAction(nameof(Index));
    }
}
