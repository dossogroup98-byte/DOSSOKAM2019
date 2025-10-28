using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DOSSOKAM2019.Data;
using System.Linq;
using System.Threading.Tasks;

public class KullaniciController : Controller
{
    private readonly ApplicationDbContext _sqlContext;
    private readonly PostgreSQLDbContext _postgresContext;

    public KullaniciController(ApplicationDbContext sqlContext, PostgreSQLDbContext postgresContext)
    {
        _sqlContext = sqlContext;
        _postgresContext = postgresContext;
    }

    // GET: Kullanıcı Listesi - Sadece Admin erişebilir
    public async Task<IActionResult> Index(string database = "sqlserver")
    {
        // Sadece Admin erişebilir
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        ViewBag.SelectedDatabase = database;

        if (database == "postgresql")
        {
            var kullaniciList = await _postgresContext.Kullanicilar
                .OrderBy(x => x.KullaniciID)
                .ToListAsync();
            return View(kullaniciList);
        }
        else
        {
            var kullaniciList = await _sqlContext.Kullanicilar
                .OrderBy(x => x.KullaniciID)
                .ToListAsync();
            return View(kullaniciList);
        }
    }

    // GET: Yeni Kullanıcı Formu
    public IActionResult Create(string database = "sqlserver")
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        ViewBag.SelectedDatabase = database;
        return View();
    }

    // POST: Yeni Kullanıcı Ekleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Kullanici kullanici, string database = "sqlserver")
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        if (ModelState.IsValid)
        {
            if (database == "postgresql")
            {
                // PostgreSQL'de aynı kullanıcı adı kontrolü
                var existingUser = await _postgresContext.Kullanicilar
                    .FirstOrDefaultAsync(x => x.KullaniciAdi == kullanici.KullaniciAdi);

                if (existingUser != null)
                {
                    ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı PostgreSQL'de zaten kullanılıyor!");
                    ViewBag.SelectedDatabase = database;
                    return View(kullanici);
                }

                kullanici.KayitTarihi = DateTime.Now;
                kullanici.Aktif = true;

                _postgresContext.Add(kullanici);
                await _postgresContext.SaveChangesAsync();

                TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı PostgreSQL'e başarıyla eklendi!";
            }
            else
            {
                // SQL Server'da aynı kullanıcı adı kontrolü
                var existingUser = await _sqlContext.Kullanicilar
                    .FirstOrDefaultAsync(x => x.KullaniciAdi == kullanici.KullaniciAdi);

                if (existingUser != null)
                {
                    ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı SQL Server'da zaten kullanılıyor!");
                    ViewBag.SelectedDatabase = database;
                    return View(kullanici);
                }

                kullanici.KayitTarihi = DateTime.Now;
                kullanici.Aktif = true;

                _sqlContext.Add(kullanici);
                await _sqlContext.SaveChangesAsync();

                TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı SQL Server'a başarıyla eklendi!";
            }
            return RedirectToAction(nameof(Index), new { database = database });
        }

        ViewBag.SelectedDatabase = database;
        return View(kullanici);
    }

    // GET: Kullanıcı Düzenleme Formu
    public async Task<IActionResult> Edit(int id, string database = "sqlserver")
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        ViewBag.SelectedDatabase = database;

        if (database == "postgresql")
        {
            var kullanici = await _postgresContext.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                TempData["Error"] = "Kullanıcı PostgreSQL'de bulunamadı!";
                return RedirectToAction(nameof(Index), new { database = database });
            }
            return View(kullanici);
        }
        else
        {
            var kullanici = await _sqlContext.Kullanicilar.FindAsync(id);
            if (kullanici == null)
            {
                TempData["Error"] = "Kullanıcı SQL Server'da bulunamadı!";
                return RedirectToAction(nameof(Index), new { database = database });
            }
            return View(kullanici);
        }
    }

    // POST: Kullanıcı Güncelleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Kullanici kullanici, string database = "sqlserver")
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        if (id != kullanici.KullaniciID)
        {
            TempData["Error"] = "Kullanıcı ID uyuşmuyor!";
            return RedirectToAction(nameof(Index), new { database = database });
        }

        if (ModelState.IsValid)
        {
            try
            {
                if (database == "postgresql")
                {
                    // PostgreSQL'de aynı kullanıcı adı kontrolü (kendisi hariç)
                    var existingUser = await _postgresContext.Kullanicilar
                        .FirstOrDefaultAsync(x => x.KullaniciAdi == kullanici.KullaniciAdi && x.KullaniciID != id);

                    if (existingUser != null)
                    {
                        ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı PostgreSQL'de zaten kullanılıyor!");
                        ViewBag.SelectedDatabase = database;
                        return View(kullanici);
                    }

                    _postgresContext.Update(kullanici);
                    await _postgresContext.SaveChangesAsync();

                    TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı PostgreSQL'de başarıyla güncellendi!";
                }
                else
                {
                    // SQL Server'da aynı kullanıcı adı kontrolü (kendisi hariç)
                    var existingUser = await _sqlContext.Kullanicilar
                        .FirstOrDefaultAsync(x => x.KullaniciAdi == kullanici.KullaniciAdi && x.KullaniciID != id);

                    if (existingUser != null)
                    {
                        ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı SQL Server'da zaten kullanılıyor!");
                        ViewBag.SelectedDatabase = database;
                        return View(kullanici);
                    }

                    _sqlContext.Update(kullanici);
                    await _sqlContext.SaveChangesAsync();

                    TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı SQL Server'da başarıyla güncellendi!";
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KullaniciExists(id, database))
                {
                    TempData["Error"] = "Kullanıcı bulunamadı!";
                    return RedirectToAction(nameof(Index), new { database = database });
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index), new { database = database });
        }

        ViewBag.SelectedDatabase = database;
        return View(kullanici);
    }

    // POST: Kullanıcı Silme
    [HttpPost]
    public async Task<IActionResult> Delete(int id, string database = "sqlserver")
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        if (database == "postgresql")
        {
            var kullanici = await _postgresContext.Kullanicilar.FindAsync(id);
            if (kullanici != null)
            {
                // Kendi hesabını silme kontrolü
                var currentUserId = HttpContext.Session.GetString("KullaniciID");
                if (kullanici.KullaniciID.ToString() == currentUserId)
                {
                    TempData["Error"] = "Kendi kullanıcı hesabınızı silemezsiniz!";
                    return RedirectToAction(nameof(Index), new { database = database });
                }

                _postgresContext.Kullanicilar.Remove(kullanici);
                await _postgresContext.SaveChangesAsync();

                TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı PostgreSQL'den başarıyla silindi!";
            }
        }
        else
        {
            var kullanici = await _sqlContext.Kullanicilar.FindAsync(id);
            if (kullanici != null)
            {
                // Kendi hesabını silme kontrolü
                var currentUserId = HttpContext.Session.GetString("KullaniciID");
                if (kullanici.KullaniciID.ToString() == currentUserId)
                {
                    TempData["Error"] = "Kendi kullanıcı hesabınızı silemezsiniz!";
                    return RedirectToAction(nameof(Index), new { database = database });
                }

                _sqlContext.Kullanicilar.Remove(kullanici);
                await _sqlContext.SaveChangesAsync();

                TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı SQL Server'dan başarıyla silindi!";
            }
        }
        return RedirectToAction(nameof(Index), new { database = database });
    }

    // POST: Kullanıcı Aktif/Pasif Yapma
    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id, string database = "sqlserver")
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        if (database == "postgresql")
        {
            var kullanici = await _postgresContext.Kullanicilar.FindAsync(id);
            if (kullanici != null)
            {
                // Kendi hesabını pasif yapma kontrolü
                var currentUserId = HttpContext.Session.GetString("KullaniciID");
                if (kullanici.KullaniciID.ToString() == currentUserId)
                {
                    TempData["Error"] = "Kendi kullanıcı hesabınızı pasif yapamazsınız!";
                    return RedirectToAction(nameof(Index), new { database = database });
                }

                kullanici.Aktif = !kullanici.Aktif;
                await _postgresContext.SaveChangesAsync();

                var durum = kullanici.Aktif ? "aktif" : "pasif";
                TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı PostgreSQL'de {durum} yapıldı!";
            }
        }
        else
        {
            var kullanici = await _sqlContext.Kullanicilar.FindAsync(id);
            if (kullanici != null)
            {
                // Kendi hesabını pasif yapma kontrolü
                var currentUserId = HttpContext.Session.GetString("KullaniciID");
                if (kullanici.KullaniciID.ToString() == currentUserId)
                {
                    TempData["Error"] = "Kendi kullanıcı hesabınızı pasif yapamazsınız!";
                    return RedirectToAction(nameof(Index), new { database = database });
                }

                kullanici.Aktif = !kullanici.Aktif;
                await _sqlContext.SaveChangesAsync();

                var durum = kullanici.Aktif ? "aktif" : "pasif";
                TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı SQL Server'da {durum} yapıldı!";
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

    private bool KullaniciExists(int id, string database)
    {
        if (database == "postgresql")
        {
            return _postgresContext.Kullanicilar.Any(e => e.KullaniciID == id);
        }
        else
        {
            return _sqlContext.Kullanicilar.Any(e => e.KullaniciID == id);
        }
    }
}