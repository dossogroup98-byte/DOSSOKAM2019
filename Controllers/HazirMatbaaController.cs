using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DOSSOKAM2019.Data;
using System.Linq;
using System.Threading.Tasks;

public class HazirMatbaaController : Controller
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly PostgreSQLDbContext _postgresContext;

    public HazirMatbaaController(ApplicationDbContext sqlContext, PostgreSQLDbContext postgresContext)
    {
        _sqlContext = sqlContext;
        _postgresContext = postgresContext;
    }

    // GET: HazirMatbaa Listesi
    public async Task<IActionResult> Index(string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (database == "postgresql")
        {
            var hazirMatbaaList = await _postgresContext.HazirMatbaa
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(hazirMatbaaList);
        }
        else
        {
            var hazirMatbaaList = await _sqlContext.HazirMatbaa
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(hazirMatbaaList);
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
    public async Task<IActionResult> Create(HazirMatbaa hazirMatbaa, string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (ModelState.IsValid)
        {
            hazirMatbaa.KayitTarihi = DateTime.Now;

            if (database == "postgresql")
            {
                _postgresContext.Add(hazirMatbaa);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Hazır Matbaa kaydı PostgreSQL'e başarıyla eklendi!";
            }
            else
            {
                _sqlContext.Add(hazirMatbaa);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Hazır Matbaa kaydı SQL Server'a başarıyla eklendi!";
            }
            return RedirectToAction(nameof(Index), new { database = database });
        }
        return View(hazirMatbaa);
    }

    // POST: Silme İşlemi
    [HttpPost]
    public async Task<IActionResult> Delete(int id, string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var hazirMatbaa = await _postgresContext.HazirMatbaa.FindAsync(id);
            if (hazirMatbaa != null)
            {
                _postgresContext.HazirMatbaa.Remove(hazirMatbaa);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Hazır Matbaa kaydı PostgreSQL'den silindi!";
            }
        }
        else
        {
            var hazirMatbaa = await _sqlContext.HazirMatbaa.FindAsync(id);
            if (hazirMatbaa != null)
            {
                _sqlContext.HazirMatbaa.Remove(hazirMatbaa);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Hazır Matbaa kaydı SQL Server'dan silindi!";
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