using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using DOSSOKAM2019.Data;
using System.Linq;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly PostgreSQLDbContext _postgresContext;

    public AccountController(ApplicationDbContext sqlContext, PostgreSQLDbContext postgresContext)
    {
        _sqlContext = sqlContext;
        _postgresContext = postgresContext;
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string kullaniciAdi, string sifre, string database = "sqlserver")
    {
        // Hard-coded admin kullanıcısı (her iki DB için de çalışır)
        if (kullaniciAdi == "admin" && sifre == "1234")
        {
            HttpContext.Session.SetString("KullaniciAdi", kullaniciAdi);
            HttpContext.Session.SetString("Role", "Yönetici");
            HttpContext.Session.SetString("AdSoyad", "Sistem Yöneticisi");
            HttpContext.Session.SetString("SelectedDatabase", database);
            return RedirectToAction("Index", "Home", new { database = database });
        }

        // Seçilen veritabanına göre login işlemi
        if (database == "postgresql")
        {
            var user = await _postgresContext.Kullanicilar
                .FirstOrDefaultAsync(u => u.KullaniciAdi == kullaniciAdi && u.Sifre == sifre && u.Aktif);

            if (user != null)
            {
                SetUserSession(user);
                HttpContext.Session.SetString("SelectedDatabase", "postgresql");
                return RedirectToAction("Index", "Home", new { database = "postgresql" });
            }
        }
        else
        {
            var user = await _sqlContext.Kullanicilar
                .FirstOrDefaultAsync(u => u.KullaniciAdi == kullaniciAdi && u.Sifre == sifre && u.Aktif);

            if (user != null)
            {
                SetUserSession(user);
                HttpContext.Session.SetString("SelectedDatabase", "sqlserver");
                return RedirectToAction("Index", "Home", new { database = "sqlserver" });
            }
        }

        ViewBag.Error = "Kullanıcı adı veya şifre hatalı!";
        ViewBag.SelectedDatabase = database;
        return View();
    }

    private void SetUserSession(Kullanici user)
    {
        HttpContext.Session.SetString("KullaniciID", user.KullaniciID.ToString());
        HttpContext.Session.SetString("KullaniciAdi", user.KullaniciAdi);
        HttpContext.Session.SetString("Role", user.Role);
        HttpContext.Session.SetString("AdSoyad", user.AdSoyad);
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
    public async Task<IActionResult> Create(Kullanici model, string database = "sqlserver")
    {
        if (HttpContext.Session.GetString("Role") != "Yönetici")
        {
            TempData["Error"] = "Bu işlem için yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        if (ModelState.IsValid)
        {
            // Seçilen veritabanına göre işlem
            if (database == "postgresql")
            {
                var existingUser = await _postgresContext.Kullanicilar
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

                _postgresContext.Kullanicilar.Add(yeniKullanici);
                await _postgresContext.SaveChangesAsync();
            }
            else
            {
                var existingUser = await _sqlContext.Kullanicilar
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

                _sqlContext.Kullanicilar.Add(yeniKullanici);
                await _sqlContext.SaveChangesAsync();
            }

            TempData["Success"] = $"{model.AdSoyad} kullanıcısı başarıyla eklendi! ({database.ToUpper()})";
            return RedirectToAction("List", new { database = database });
        }

        return View(model);
    }

    public async Task<IActionResult> List(string database = "sqlserver")
    {
        if (HttpContext.Session.GetString("Role") != "Yönetici")
        {
            TempData["Error"] = "Bu işlem için yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        ViewBag.SelectedDatabase = database;

        if (database == "postgresql")
        {
            var kullanicilar = await _postgresContext.Kullanicilar
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(kullanicilar);
        }
        else
        {
            var kullanicilar = await _sqlContext.Kullanicilar
                .OrderByDescending(x => x.KayitTarihi)
                .ToListAsync();
            return View(kullanicilar);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id, string database = "sqlserver")
    {
        if (HttpContext.Session.GetString("Role") != "Yönetici")
        {
            TempData["Error"] = "Bu işlem için yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        if (database == "postgresql")
        {
            var kullanici = await _postgresContext.Kullanicilar.FindAsync(id);
            if (kullanici != null)
            {
                _postgresContext.Kullanicilar.Remove(kullanici);
                await _postgresContext.SaveChangesAsync();
                TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı PostgreSQL'den silindi!";
            }
        }
        else
        {
            var kullanici = await _sqlContext.Kullanicilar.FindAsync(id);
            if (kullanici != null)
            {
                _sqlContext.Kullanicilar.Remove(kullanici);
                await _sqlContext.SaveChangesAsync();
                TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı SQL Server'dan silindi!";
            }
        }

        return RedirectToAction("List", new { database = database });
    }

    // Database değiştirme endpoint'i
    [HttpPost]
    public IActionResult SwitchDatabase(string database)
    {
        HttpContext.Session.SetString("SelectedDatabase", database);
        return RedirectToAction("List", new { database = database });
    }
}