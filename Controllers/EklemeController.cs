using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DOSSOKAM2019.Data;
using System.Linq;
using System.Threading.Tasks;

public class EklemeController : Controller
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly PostgreSQLDbContext _postgresContext;

    public EklemeController(ApplicationDbContext sqlContext, PostgreSQLDbContext postgresContext)
    {
        _sqlContext = sqlContext;
        _postgresContext = postgresContext;
    }

    // GET: Ekleme Listesi
    public async Task<IActionResult> Index(string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (database == "postgresql")
        {
            var eklemeList = await _postgresContext.Ekleme
                .Where(x => !x.Tamamlandi)
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(eklemeList);
        }
        else
        {
            var eklemeList = await _sqlContext.Ekleme
                .Where(x => !x.Tamamlandi)
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(eklemeList);
        }
    }

    // GET: Yeni Kayıt Formu
    public IActionResult Create(string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;
        return View();
    }

    // POST: Yeni Kayıt Ekleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Ekleme ekleme, string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (ModelState.IsValid)
        {
            ekleme.KayitTarihi = DateTime.Now;

            if (database == "postgresql")
            {
                _postgresContext.Add(ekleme);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Ekleme kaydı PostgreSQL'e başarıyla eklendi!";
            }
            else
            {
                _sqlContext.Add(ekleme);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Ekleme kaydı SQL Server'a başarıyla eklendi!";
            }
            return RedirectToAction(nameof(Index), new { database = database });
        }
        return View(ekleme);
    }

    // POST: Makine Saat'ten Otomatik Geçiş
    [HttpPost]
    public async Task<IActionResult> AddFromMakineSaat(int makineSaatId, string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var makineSaat = await _postgresContext.MakineSaat.FindAsync(makineSaatId);
            if (makineSaat != null)
            {
                var ekleme = new Ekleme
                {
                    SiparisNo = makineSaat.SiparisNo,
                    UrunAdi = makineSaat.UrunAdi,
                    BaslamaTarihi = DateTime.Now,
                    Aciklama = $"Makine Saat'ten otomatik geçiş - {makineSaat.Aciklama}",
                    Tamamlandi = false,
                    KayitTarihi = DateTime.Now
                };

                _postgresContext.Ekleme.Add(ekleme);
                await _postgresContext.SaveChangesAsync();

                TempData["Success"] = $"{makineSaat.SiparisNo} numaralı sipariş PostgreSQL Ekleme'ye taşındı!";
            }
        }
        else
        {
            var makineSaat = await _sqlContext.MakineSaat.FindAsync(makineSaatId);
            if (makineSaat != null)
            {
                var ekleme = new Ekleme
                {
                    SiparisNo = makineSaat.SiparisNo,
                    UrunAdi = makineSaat.UrunAdi,
                    BaslamaTarihi = DateTime.Now,
                    Aciklama = $"Makine Saat'ten otomatik geçiş - {makineSaat.Aciklama}",
                    Tamamlandi = false,
                    KayitTarihi = DateTime.Now
                };

                _sqlContext.Ekleme.Add(ekleme);
                await _sqlContext.SaveChangesAsync();

                TempData["Success"] = $"{makineSaat.SiparisNo} numaralı sipariş SQL Server Ekleme'ye taşındı!";
            }
        }
        return RedirectToAction(nameof(Index), new { database = database });
    }

    // POST: Completed İşlemi
    [HttpPost]
    public async Task<IActionResult> Complete(int id, string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var ekleme = await _postgresContext.Ekleme.FindAsync(id);
            if (ekleme != null)
            {
                ekleme.Tamamlandi = true;
                ekleme.BitisTarihi = DateTime.Now;

                // OTOMATİK DAZMAL'A GEÇİŞ - PostgreSQL
                var dazmal = new Dazmal
                {
                    SiparisNo = ekleme.SiparisNo,
                    UrunAdi = ekleme.UrunAdi,
                    BaslamaTarihi = DateTime.Now,
                    Aciklama = $"Ekleme tamamlandı",
                    Tamamlandi = false,
                    KayitTarihi = DateTime.Now
                };

                _postgresContext.Dazmal.Add(dazmal);
                await _postgresContext.SaveChangesAsync();

                TempData["Success"] = $"{ekleme.SiparisNo} PostgreSQL'de tamamlandı ve Dazmal'a taşındı!";
            }
        }
        else
        {
            var ekleme = await _sqlContext.Ekleme.FindAsync(id);
            if (ekleme != null)
            {
                ekleme.Tamamlandi = true;
                ekleme.BitisTarihi = DateTime.Now;

                // OTOMATİK DAZMAL'A GEÇİŞ - SQL Server
                var dazmal = new Dazmal
                {
                    SiparisNo = ekleme.SiparisNo,
                    UrunAdi = ekleme.UrunAdi,
                    BaslamaTarihi = DateTime.Now,
                    Aciklama = $"Ekleme tamamlandı",
                    Tamamlandi = false,
                    KayitTarihi = DateTime.Now
                };

                _sqlContext.Dazmal.Add(dazmal);
                await _sqlContext.SaveChangesAsync();

                TempData["Success"] = $"{ekleme.SiparisNo} SQL Server'da tamamlandı ve Dazmal'a taşındı!";
            }
        }
        return RedirectToAction(nameof(Index), new { database = database });
    }

    // POST: Silme İşlemi
    [HttpPost]
    public async Task<IActionResult> Delete(int id, string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var ekleme = await _postgresContext.Ekleme.FindAsync(id);
            if (ekleme != null)
            {
                _postgresContext.Ekleme.Remove(ekleme);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Ekleme kaydı PostgreSQL'den silindi!";
            }
        }
        else
        {
            var ekleme = await _sqlContext.Ekleme.FindAsync(id);
            if (ekleme != null)
            {
                _sqlContext.Ekleme.Remove(ekleme);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Ekleme kaydı SQL Server'dan silindi!";
            }
        }
        return RedirectToAction(nameof(Index), new { database = database });
    }

    // GET: Temizleme (Tüm tamamlananları sil)
    public async Task<IActionResult> Clear(string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var tamamlananlar = await _postgresContext.Ekleme.Where(x => x.Tamamlandi).ToListAsync();
            _postgresContext.Ekleme.RemoveRange(tamamlananlar);
            await _postgresContext.SaveChangesAsync();
            TempData["Success"] = $"{tamamlananlar.Count} adet tamamlanmış kayıt PostgreSQL'den silindi!";
        }
        else
        {
            var tamamlananlar = await _sqlContext.Ekleme.Where(x => x.Tamamlandi).ToListAsync();
            _sqlContext.Ekleme.RemoveRange(tamamlananlar);
            await _sqlContext.SaveChangesAsync();
            TempData["Success"] = $"{tamamlananlar.Count} adet tamamlanmış kayıt SQL Server'dan silindi!";
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