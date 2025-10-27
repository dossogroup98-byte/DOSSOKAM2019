using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public AccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string kullaniciAdi, string sifre)
    {
        if (kullaniciAdi == "admin" && sifre == "1234")
        {
            HttpContext.Session.SetString("KullaniciAdi", kullaniciAdi);
            HttpContext.Session.SetString("Role", "Yönetici");
            HttpContext.Session.SetString("AdSoyad", "Sistem Yöneticisi");
            return RedirectToAction("Index", "Home");
        }

        var user = await _context.Kullanicilar
            .FirstOrDefaultAsync(u => u.KullaniciAdi == kullaniciAdi && u.Sifre == sifre && u.Aktif);

        if (user != null)
        {
            HttpContext.Session.SetString("KullaniciID", user.KullaniciID.ToString());
            HttpContext.Session.SetString("KullaniciAdi", user.KullaniciAdi);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("AdSoyad", user.AdSoyad);
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
        return View();
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Role") != "Yönetici")
        {
            TempData["Error"] = "Bu işlem için yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Kullanici model)
    {
        if (HttpContext.Session.GetString("Role") != "Yönetici")
        {
            TempData["Error"] = "Bu işlem için yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        if (ModelState.IsValid)
        {
            var existingUser = await _context.Kullanicilar
                .FirstOrDefaultAsync(u => u.KullaniciAdi == model.KullaniciAdi);

            if (existingUser != null)
            {
                ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı zaten kullanılıyor.");
                return View(model);
            }

            var yeniKullanici = new Kullanici
            {
                AdSoyad = model.AdSoyad,
                KullaniciAdi = model.KullaniciAdi,
                Sifre = model.Sifre,
                Role = model.Role,
                Aktif = true,
                KayitTarihi = DateTime.Now
            };

            _context.Kullanicilar.Add(yeniKullanici);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{model.AdSoyad} kullanıcısı başarıyla eklendi!";
            return RedirectToAction("List");
        }

        return View(model);
    }

    public async Task<IActionResult> List()
    {
        if (HttpContext.Session.GetString("Role") != "Yönetici")
        {
            TempData["Error"] = "Bu işlem için yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        var kullanicilar = await _context.Kullanicilar
            .OrderByDescending(x => x.KayitTarihi)
            .ToListAsync();
        return View(kullanicilar);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (HttpContext.Session.GetString("Role") != "Yönetici")
        {
            TempData["Error"] = "Bu işlem için yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        var kullanici = await _context.Kullanicilar.FindAsync(id);
        if (kullanici != null)
        {
            _context.Kullanicilar.Remove(kullanici);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı silindi!";
        }
        return RedirectToAction("List");
    }
}