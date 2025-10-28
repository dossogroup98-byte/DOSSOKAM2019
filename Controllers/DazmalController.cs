using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DOSSOKAM2019.Data;
using System.Linq;
using System.Threading.Tasks;

public class DazmalController : Controller
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly PostgreSQLDbContext _postgresContext;

    public DazmalController(ApplicationDbContext sqlContext, PostgreSQLDbContext postgresContext)
    {
        _sqlContext = sqlContext;
        _postgresContext = postgresContext;
    }

    // GET: Dazmal Listesi
    public async Task<IActionResult> Index(string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (database == "postgresql")
        {
            var dazmalList = await _postgresContext.Dazmal
                .Where(x => !x.Tamamlandi)
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(dazmalList);
        }
        else
        {
            var dazmalList = await _sqlContext.Dazmal
                .Where(x => !x.Tamamlandi)
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(dazmalList);
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
    public async Task<IActionResult> Create(Dazmal dazmal, string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (ModelState.IsValid)
        {
            dazmal.KayitTarihi = DateTime.Now;

            if (database == "postgresql")
            {
                _postgresContext.Add(dazmal);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Dazmal kaydı PostgreSQL'e başarıyla eklendi!";
            }
            else
            {
                _sqlContext.Add(dazmal);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Dazmal kaydı SQL Server'a başarıyla eklendi!";
            }
            return RedirectToAction(nameof(Index), new { database = database });
        }
        return View(dazmal);
    }

    // POST: Ekleme'den Otomatik Geçiş
    [HttpPost]
    public async Task<IActionResult> AddFromEkleme(int eklemeId, string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var ekleme = await _postgresContext.Ekleme.FindAsync(eklemeId);
            if (ekleme != null)
            {
                var dazmal = new Dazmal
                {
                    SiparisNo = ekleme.SiparisNo,
                    UrunAdi = ekleme.UrunAdi,
                    BaslamaTarihi = DateTime.Now,
                    Aciklama = $"Ekleme'den otomatik geçiş - {ekleme.Aciklama}",
                    Tamamlandi = false,
                    KayitTarihi = DateTime.Now
                };

                _postgresContext.Dazmal.Add(dazmal);
                await _postgresContext.SaveChangesAsync();

                TempData["Success"] = $"{ekleme.SiparisNo} numaralı sipariş PostgreSQL Dazmal'a taşındı!";
            }
        }
        else
        {
            var ekleme = await _sqlContext.Ekleme.FindAsync(eklemeId);
            if (ekleme != null)
            {
                var dazmal = new Dazmal
                {
                    SiparisNo = ekleme.SiparisNo,
                    UrunAdi = ekleme.UrunAdi,
                    BaslamaTarihi = DateTime.Now,
                    Aciklama = $"Ekleme'den otomatik geçiş - {ekleme.Aciklama}",
                    Tamamlandi = false,
                    KayitTarihi = DateTime.Now
                };

                _sqlContext.Dazmal.Add(dazmal);
                await _sqlContext.SaveChangesAsync();

                TempData["Success"] = $"{ekleme.SiparisNo} numaralı sipariş SQL Server Dazmal'a taşındı!";
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
            var dazmal = await _postgresContext.Dazmal.FindAsync(id);
            if (dazmal != null)
            {
                dazmal.Tamamlandi = true;
                dazmal.BitisTarihi = DateTime.Now;
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Dazmal işlemi PostgreSQL'de tamamlandı!";
            }
        }
        else
        {
            var dazmal = await _sqlContext.Dazmal.FindAsync(id);
            if (dazmal != null)
            {
                dazmal.Tamamlandi = true;
                dazmal.BitisTarihi = DateTime.Now;
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Dazmal işlemi SQL Server'da tamamlandı!";
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
            var dazmal = await _postgresContext.Dazmal.FindAsync(id);
            if (dazmal != null)
            {
                _postgresContext.Dazmal.Remove(dazmal);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Dazmal kaydı PostgreSQL'den silindi!";
            }
        }
        else
        {
            var dazmal = await _sqlContext.Dazmal.FindAsync(id);
            if (dazmal != null)
            {
                _sqlContext.Dazmal.Remove(dazmal);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Dazmal kaydı SQL Server'dan silindi!";
            }
        }
        return RedirectToAction(nameof(Index), new { database = database });
    }

    // GET: Temizleme (Tüm tamamlananları sil)
    public async Task<IActionResult> Clear(string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var tamamlananlar = await _postgresContext.Dazmal.Where(x => x.Tamamlandi).ToListAsync();
            _postgresContext.Dazmal.RemoveRange(tamamlananlar);
            await _postgresContext.SaveChangesAsync();
            TempData["Success"] = $"{tamamlananlar.Count} adet tamamlanmış kayıt PostgreSQL'den silindi!";
        }
        else
        {
            var tamamlananlar = await _sqlContext.Dazmal.Where(x => x.Tamamlandi).ToListAsync();
            _sqlContext.Dazmal.RemoveRange(tamamlananlar);
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