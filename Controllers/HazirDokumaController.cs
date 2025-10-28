using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DOSSOKAM2019.Data;
using System.Linq;
using System.Threading.Tasks;

public class HazirDokumaController : Controller
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly PostgreSQLDbContext _postgresContext;

    public HazirDokumaController(ApplicationDbContext sqlContext, PostgreSQLDbContext postgresContext)
    {
        _sqlContext = sqlContext;
        _postgresContext = postgresContext;
    }

    // GET: HazirDokuma Listesi
    public async Task<IActionResult> Index(string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (database == "postgresql")
        {
            var hazirDokumaList = await _postgresContext.HazirDokuma
                .Where(x => !x.Tamamlandi)
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(hazirDokumaList);
        }
        else
        {
            var hazirDokumaList = await _sqlContext.HazirDokuma
                .Where(x => !x.Tamamlandi)
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(hazirDokumaList);
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
    public async Task<IActionResult> Create(HazirDokuma hazirDokuma, string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (ModelState.IsValid)
        {
            // Computed column'ları NULL yap
            hazirDokuma.GecenSure = null;
            hazirDokuma.KayitTarihi = DateTime.Now;

            if (database == "postgresql")
            {
                _postgresContext.Add(hazirDokuma);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Hazır Dokuma kaydı PostgreSQL'e başarıyla eklendi!";
            }
            else
            {
                _sqlContext.Add(hazirDokuma);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Hazır Dokuma kaydı SQL Server'a başarıyla eklendi!";
            }
            return RedirectToAction(nameof(Index), new { database = database });
        }
        return View(hazirDokuma);
    }

    // POST: Tamamlama İşlemi
    [HttpPost]
    public async Task<IActionResult> Complete(int id, string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var hazirDokuma = await _postgresContext.HazirDokuma.FindAsync(id);
            if (hazirDokuma != null)
            {
                hazirDokuma.Tamamlandi = true;
                hazirDokuma.BitisTarihi = DateTime.Now;

                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Hazır Dokuma işlemi PostgreSQL'de tamamlandı!";
            }
        }
        else
        {
            var hazirDokuma = await _sqlContext.HazirDokuma.FindAsync(id);
            if (hazirDokuma != null)
            {
                hazirDokuma.Tamamlandi = true;
                hazirDokuma.BitisTarihi = DateTime.Now;

                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Hazır Dokuma işlemi SQL Server'da tamamlandı!";
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
            var hazirDokuma = await _postgresContext.HazirDokuma.FindAsync(id);
            if (hazirDokuma != null)
            {
                _postgresContext.HazirDokuma.Remove(hazirDokuma);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Hazır Dokuma kaydı PostgreSQL'den silindi!";
            }
        }
        else
        {
            var hazirDokuma = await _sqlContext.HazirDokuma.FindAsync(id);
            if (hazirDokuma != null)
            {
                _sqlContext.HazirDokuma.Remove(hazirDokuma);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Hazır Dokuma kaydı SQL Server'dan silindi!";
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