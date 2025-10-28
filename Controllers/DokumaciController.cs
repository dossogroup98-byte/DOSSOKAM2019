using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DOSSOKAM2019.Data;
using System.Threading.Tasks;

public class DokumaciController : Controller
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly PostgreSQLDbContext _postgresContext;

    public DokumaciController(ApplicationDbContext sqlContext, PostgreSQLDbContext postgresContext)
    {
        _sqlContext = sqlContext;
        _postgresContext = postgresContext;
    }

    // GET: Dokumaci
    public async Task<IActionResult> Index(string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (database == "postgresql")
        {
            var dokumacilar = await _postgresContext.Dokumacilar.ToListAsync();
            return View(dokumacilar);
        }
        else
        {
            var dokumacilar = await _sqlContext.Dokumacilar.ToListAsync();
            return View(dokumacilar);
        }
    }

    // GET: Dokumaci/Create
    public IActionResult Create(string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;
        return View();
    }

    // POST: Dokumaci/Create
    [HttpPost]
    public async Task<IActionResult> Create(Dokumaci dokumaci, string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (ModelState.IsValid)
        {
            dokumaci.KayitTarihi = DateTime.Now;

            if (database == "postgresql")
            {
                _postgresContext.Add(dokumaci);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Dokumacı PostgreSQL'e başarıyla eklendi!";
            }
            else
            {
                _sqlContext.Add(dokumaci);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Dokumacı SQL Server'a başarıyla eklendi!";
            }
            return RedirectToAction(nameof(Index), new { database = database });
        }
        return View(dokumaci);
    }

    // GET: Dokumaci/Edit/5
    public async Task<IActionResult> Edit(int? id, string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (id == null) return NotFound();

        if (database == "postgresql")
        {
            var dokumaci = await _postgresContext.Dokumacilar.FindAsync(id);
            if (dokumaci == null) return NotFound();
            return View(dokumaci);
        }
        else
        {
            var dokumaci = await _sqlContext.Dokumacilar.FindAsync(id);
            if (dokumaci == null) return NotFound();
            return View(dokumaci);
        }
    }

    // POST: Dokumaci/Edit/5
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Dokumaci dokumaci, string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (id != dokumaci.DokumaciID) return NotFound();

        if (ModelState.IsValid)
        {
            if (database == "postgresql")
            {
                _postgresContext.Update(dokumaci);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Dokumacı PostgreSQL'de başarıyla güncellendi!";
            }
            else
            {
                _sqlContext.Update(dokumaci);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Dokumacı SQL Server'da başarıyla güncellendi!";
            }
            return RedirectToAction(nameof(Index), new { database = database });
        }
        return View(dokumaci);
    }

    // GET: Dokumaci/Delete/5
    public async Task<IActionResult> Delete(int? id, string database = "sqlserver")
    {
        ViewBag.SelectedDatabase = database;

        if (id == null) return NotFound();

        if (database == "postgresql")
        {
            var dokumaci = await _postgresContext.Dokumacilar.FindAsync(id);
            if (dokumaci == null) return NotFound();
            return View(dokumaci);
        }
        else
        {
            var dokumaci = await _sqlContext.Dokumacilar.FindAsync(id);
            if (dokumaci == null) return NotFound();
            return View(dokumaci);
        }
    }

    // POST: Dokumaci/Delete/5
    [HttpPost, ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(int id, string database = "sqlserver")
    {
        if (database == "postgresql")
        {
            var dokumaci = await _postgresContext.Dokumacilar.FindAsync(id);
            if (dokumaci != null)
            {
                _postgresContext.Dokumacilar.Remove(dokumaci);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = "Dokumacı PostgreSQL'den başarıyla silindi!";
            }
        }
        else
        {
            var dokumaci = await _sqlContext.Dokumacilar.FindAsync(id);
            if (dokumaci != null)
            {
                _sqlContext.Dokumacilar.Remove(dokumaci);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = "Dokumacı SQL Server'dan başarıyla silindi!";
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