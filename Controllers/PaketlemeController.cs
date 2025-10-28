using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DOSSOKAM2019.Data;
using System.Linq;
using System.Threading.Tasks;

public class PaketlemeController : Controller
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly PostgreSQLDbContext _postgresContext;

    public PaketlemeController(ApplicationDbContext sqlContext, PostgreSQLDbContext postgresContext)
    {
        _sqlContext = sqlContext;
        _postgresContext = postgresContext;
    }

    // GET: Paketleme Listesi
    public async Task<IActionResult> Index(string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (database == "postgresql")
        {
            var paketlemeList = await _postgresContext.Paketleme
                .Where(x => !x.Tamamlandi)
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(paketlemeList);
        }
        else
        {
            var paketlemeList = await _sqlContext.Paketleme
                .Where(x => !x.Tamamlandi)
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(paketlemeList);
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
    public async Task<IActionResult> Create(Paketleme paketleme, string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (ModelState.IsValid)
        {
            paketleme.KayitTarihi = DateTime.Now;

            if (database == "postgresql")
            {
                _postgresContext.Add(paketleme);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Paketleme kaydı PostgreSQL'e başarıyla eklendi!";
            }
            else
            {
                _sqlContext.Add(paketleme);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Paketleme kaydı SQL Server'a başarıyla eklendi!";
            }
            return RedirectToAction(nameof(Index), new { database = database });
        }
        return View(paketleme);
    }

    // POST: Tamamlama İşlemi
    [HttpPost]
    public async Task<IActionResult> Complete(int id, string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var paketleme = await _postgresContext.Paketleme.FindAsync(id);
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

                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = $"Paketleme işlemi PostgreSQL'de tamamlandı! Toplam süre: {paketleme.GecenSure:F2} saat";
            }
        }
        else
        {
            var paketleme = await _sqlContext.Paketleme.FindAsync(id);
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

                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = $"Paketleme işlemi SQL Server'da tamamlandı! Toplam süre: {paketleme.GecenSure:F2} saat";
            }
        }
        return RedirectToAction(nameof(Index), new { database = database });
    }

    private decimal CalculateTimeDifferenceInHours(DateTime baslangic, DateTime bitis)
    {
        var fark = bitis - baslangic;
        return (decimal)fark.TotalHours;
    }

    // POST: Silme İşlemi
    [HttpPost]
    public async Task<IActionResult> Delete(int id, string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var paketleme = await _postgresContext.Paketleme.FindAsync(id);
            if (paketleme != null)
            {
                _postgresContext.Paketleme.Remove(paketleme);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Paketleme kaydı PostgreSQL'den silindi!";
            }
        }
        else
        {
            var paketleme = await _sqlContext.Paketleme.FindAsync(id);
            if (paketleme != null)
            {
                _sqlContext.Paketleme.Remove(paketleme);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Paketleme kaydı SQL Server'dan silindi!";
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