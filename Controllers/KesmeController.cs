using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DOSSOKAM2019.Data;
using System.Linq;
using System.Threading.Tasks;

public class KesmeController : Controller
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly PostgreSQLDbContext _postgresContext;

    public KesmeController(ApplicationDbContext sqlContext, PostgreSQLDbContext postgresContext)
    {
        _sqlContext = sqlContext;
        _postgresContext = postgresContext;
    }

    // GET: Kesme Listesi
    public async Task<IActionResult> Index(string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (database == "postgresql")
        {
            var kesmeList = await _postgresContext.Kesme
                .Where(x => !x.Tamamlandi)
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(kesmeList);
        }
        else
        {
            var kesmeList = await _sqlContext.Kesme
                .Where(x => !x.Tamamlandi)
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(kesmeList);
        }
    }

    // GET: Yeni Kayıt Formu
    public IActionResult Create(string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;
        return View();
    }

    // POST: Yeni Kayıt Ekleme - GECENSURE OLMADAN
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Kesme kesme, string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (ModelState.IsValid)
        {
            // GecenSure'ü NULL yap (computed column)
            kesme.GecenSure = null;
            kesme.KayitTarihi = DateTime.Now;

            if (database == "postgresql")
            {
                _postgresContext.Add(kesme);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Kesme kaydı PostgreSQL'e başarıyla eklendi!";
            }
            else
            {
                _sqlContext.Add(kesme);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Kesme kaydı SQL Server'a başarıyla eklendi!";
            }
            return RedirectToAction(nameof(Index), new { database = database });
        }
        return View(kesme);
    }

    // POST: Tamamlama İşlemi - GECENSURE GÜNCELLEME YOK
    [HttpPost]
    public async Task<IActionResult> Complete(int id, string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var kesme = await _postgresContext.Kesme.FindAsync(id);
            if (kesme != null)
            {
                kesme.Tamamlandi = true;
                kesme.BitisTarihi = DateTime.Now;

                // GECENSURE GÜNCELLEME YOK - computed column olduğu için
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Kesme işlemi PostgreSQL'de tamamlandı!";
            }
        }
        else
        {
            var kesme = await _sqlContext.Kesme.FindAsync(id);
            if (kesme != null)
            {
                kesme.Tamamlandi = true;
                kesme.BitisTarihi = DateTime.Now;

                // GECENSURE GÜNCELLEME YOK - computed column olduğu için
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Kesme işlemi SQL Server'da tamamlandı!";
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
            var kesme = await _postgresContext.Kesme.FindAsync(id);
            if (kesme != null)
            {
                _postgresContext.Kesme.Remove(kesme);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Kesme kaydı PostgreSQL'den silindi!";
            }
        }
        else
        {
            var kesme = await _sqlContext.Kesme.FindAsync(id);
            if (kesme != null)
            {
                _sqlContext.Kesme.Remove(kesme);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Kesme kaydı SQL Server'dan silindi!";
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