using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DOSSOKAM2019.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

public class MakineSaatController : Controller
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly PostgreSQLDbContext _postgresContext;

    public MakineSaatController(ApplicationDbContext sqlContext, PostgreSQLDbContext postgresContext)
    {
        _sqlContext = sqlContext;
        _postgresContext = postgresContext;
    }

    // GET: MakineSaat Listesi
    public async Task<IActionResult> Index(string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (database == "postgresql")
        {
            var makineSaatList = await _postgresContext.MakineSaat
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(makineSaatList);
        }
        else
        {
            var makineSaatList = await _sqlContext.MakineSaat
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(makineSaatList);
        }
    }

    // GET: Yeni Kayıt Formu
    public IActionResult Create(string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        // ✅ DOKUMACILARI VIEWBAG'E EKLE
        if (database == "postgresql")
        {
            ViewBag.Dokumacilar = _postgresContext.Dokumacilar.Where(d => d.Aktif).ToList();
        }
        else
        {
            ViewBag.Dokumacilar = _sqlContext.Dokumacilar.Where(d => d.Aktif).ToList();
        }
        return View();
    }

    // POST: Yeni Kayıt Ekleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MakineSaat makineSaat, string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (ModelState.IsValid)
        {
            makineSaat.KayitTarihi = DateTime.Now;

            if (database == "postgresql")
            {
                _postgresContext.Add(makineSaat);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Makine Saat kaydı PostgreSQL'e başarıyla eklendi!";
            }
            else
            {
                _sqlContext.Add(makineSaat);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Makine Saat kaydı SQL Server'a başarıyla eklendi!";
            }
            return RedirectToAction(nameof(Index), new { database = database });
        }

        // ✅ FORM HATALIYSA DOKUMACILARI TEKRAR YÜKLE
        if (database == "postgresql")
        {
            ViewBag.Dokumacilar = _postgresContext.Dokumacilar.Where(d => d.Aktif).ToList();
        }
        else
        {
            ViewBag.Dokumacilar = _sqlContext.Dokumacilar.Where(d => d.Aktif).ToList();
        }
        return View(makineSaat);
    }

    // POST: Tamamlama İşlemi - EKLEME'YE TRANSFER
    [HttpPost]
    public async Task<IActionResult> Complete(int id, string database = "sqlserver")
    {
        try
        {
            if (database == "postgresql")
            {
                var makineSaat = await _postgresContext.MakineSaat.FindAsync(id);
                if (makineSaat == null)
                {
                    TempData["Error"] = "Kayıt PostgreSQL'de bulunamadı!";
                    return RedirectToAction(nameof(Index), new { database = database });
                }

                // 1. Ekleme'ye yeni kayıt oluştur - PostgreSQL
                var ekleme = new Ekleme
                {
                    SiparisNo = makineSaat.SiparisNo,
                    UrunAdi = makineSaat.UrunAdi,
                    BaslamaTarihi = DateTime.Now,
                    Aciklama = $"Makine Saat'ten otomatik geçiş - Kalan Saat: {makineSaat.KalanSaat}",
                    Tamamlandi = false,
                    KayitTarihi = DateTime.Now
                };

                _postgresContext.Ekleme.Add(ekleme);

                // 2. MakineSaat'ten KAYDI SİL - PostgreSQL
                _postgresContext.MakineSaat.Remove(makineSaat);

                // 3. Her ikisini de kaydet - PostgreSQL
                await _postgresContext.SaveChangesAsync();

                TempData["Success"] = $"{makineSaat.SiparisNo} PostgreSQL Ekleme'ye taşındı ve listeden kaldırıldı!";
            }
            else
            {
                var makineSaat = await _sqlContext.MakineSaat.FindAsync(id);
                if (makineSaat == null)
                {
                    TempData["Error"] = "Kayıt SQL Server'da bulunamadı!";
                    return RedirectToAction(nameof(Index), new { database = database });
                }

                // 1. Ekleme'ye yeni kayıt oluştur - SQL Server
                var ekleme = new Ekleme
                {
                    SiparisNo = makineSaat.SiparisNo,
                    UrunAdi = makineSaat.UrunAdi,
                    BaslamaTarihi = DateTime.Now,
                    Aciklama = $"Makine Saat'ten otomatik geçiş - Kalan Saat: {makineSaat.KalanSaat}",
                    Tamamlandi = false,
                    KayitTarihi = DateTime.Now
                };

                _sqlContext.Ekleme.Add(ekleme);

                // 2. MakineSaat'ten KAYDI SİL - SQL Server
                _sqlContext.MakineSaat.Remove(makineSaat);

                // 3. Her ikisini de kaydet - SQL Server
                await _sqlContext.SaveChangesAsync();

                TempData["Success"] = $"{makineSaat.SiparisNo} SQL Server Ekleme'ye taşındı ve listeden kaldırıldı!";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Hata: {ex.Message}";
        }
        return RedirectToAction(nameof(Index), new { database = database });
    }

    // POST: Silme İşlemi
    [HttpPost]
    public async Task<IActionResult> Delete(int id, string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var makineSaat = await _postgresContext.MakineSaat.FindAsync(id);
            if (makineSaat != null)
            {
                _postgresContext.MakineSaat.Remove(makineSaat);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Makine Saat kaydı PostgreSQL'den silindi!";
            }
        }
        else
        {
            var makineSaat = await _sqlContext.MakineSaat.FindAsync(id);
            if (makineSaat != null)
            {
                _sqlContext.MakineSaat.Remove(makineSaat);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Makine Saat kaydı SQL Server'dan silindi!";
            }
        }
        return RedirectToAction(nameof(Index), new { database = database });
    }

    // Database değiştirme endpoint'i
    [HttpPost]
    public IActionResult SwitchDatabase(string database)
    {
        return RedirectToAction("Index", new { database = database });
    }
}