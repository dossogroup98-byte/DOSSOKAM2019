using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class HazirMatbaaController : Controller
{
    private readonly ApplicationDbContext _context;

    public HazirMatbaaController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: HazirMatbaa Listesi
    public async Task<IActionResult> Index()
    {
        var hazirMatbaaList = await _context.HazirMatbaa
            .OrderByDescending(x => x.KayitTarihi)
            .ToListAsync();
        return View(hazirMatbaaList);
    }

    // GET: Yeni Kayıt Formu
    public IActionResult Create()
    {
        return View();
    }

    // POST: Yeni Kayıt Ekleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HazirMatbaa hazirMatbaa)
    {
        if (ModelState.IsValid)
        {
            _context.Add(hazirMatbaa);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Hazır Matbaa kaydı başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(hazirMatbaa);
    }

    // POST: Silme İşlemi
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var hazirMatbaa = await _context.HazirMatbaa.FindAsync(id);
        if (hazirMatbaa != null)
        {
            _context.HazirMatbaa.Remove(hazirMatbaa);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Hazır Matbaa kaydı silindi!";
        }
        return RedirectToAction(nameof(Index));
    }
}