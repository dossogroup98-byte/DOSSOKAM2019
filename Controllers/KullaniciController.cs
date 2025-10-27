using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

public class KullaniciController : Controller
{
    private readonly ApplicationDbContext _context;

    public KullaniciController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Kullanıcı Listesi - Sadece Admin erişebilir
    public async Task<IActionResult> Index()
    {
        // Sadece Admin erişebilir
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        var kullaniciList = await _context.Kullanicilar
            .OrderBy(x => x.KullaniciID)
            .ToListAsync();
        return View(kullaniciList);
    }

    // GET: Yeni Kullanıcı Formu
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    // POST: Yeni Kullanıcı Ekleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Kullanici kullanici)
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        if (ModelState.IsValid)
        {
            // Aynı kullanıcı adı kontrolü
            var existingUser = await _context.Kullanicilar
                .FirstOrDefaultAsync(x => x.KullaniciAdi == kullanici.KullaniciAdi);

            if (existingUser != null)
            {
                ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı zaten kullanılıyor!");
                return View(kullanici);
            }

            kullanici.KayitTarihi = DateTime.Now;
            kullanici.Aktif = true;

            _context.Add(kullanici);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı başarıyla eklendi!";
            return RedirectToAction(nameof(Index));
        }
        return View(kullanici);
    }

    // GET: Kullanıcı Düzenleme Formu
    public async Task<IActionResult> Edit(int id)
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        var kullanici = await _context.Kullanicilar.FindAsync(id);
        if (kullanici == null)
        {
            TempData["Error"] = "Kullanıcı bulunamadı!";
            return RedirectToAction(nameof(Index));
        }
        return View(kullanici);
    }

    // POST: Kullanıcı Güncelleme
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Kullanici kullanici)
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        if (id != kullanici.KullaniciID)
        {
            TempData["Error"] = "Kullanıcı ID uyuşmuyor!";
            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Aynı kullanıcı adı kontrolü (kendisi hariç)
                var existingUser = await _context.Kullanicilar
                    .FirstOrDefaultAsync(x => x.KullaniciAdi == kullanici.KullaniciAdi && x.KullaniciID != id);

                if (existingUser != null)
                {
                    ModelState.AddModelError("KullaniciAdi", "Bu kullanıcı adı zaten kullanılıyor!");
                    return View(kullanici);
                }

                _context.Update(kullanici);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı başarıyla güncellendi!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!KullaniciExists(kullanici.KullaniciID))
                {
                    TempData["Error"] = "Kullanıcı bulunamadı!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(kullanici);
    }

    // POST: Kullanıcı Silme
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        var kullanici = await _context.Kullanicilar.FindAsync(id);
        if (kullanici != null)
        {
            // Kendi hesabını silme kontrolü
            var currentUserId = HttpContext.Session.GetString("KullaniciID");
            if (kullanici.KullaniciID.ToString() == currentUserId)
            {
                TempData["Error"] = "Kendi kullanıcı hesabınızı silemezsiniz!";
                return RedirectToAction(nameof(Index));
            }

            _context.Kullanicilar.Remove(kullanici);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı başarıyla silindi!";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Kullanıcı Aktif/Pasif Yapma
    [HttpPost]
    public async Task<IActionResult> ToggleStatus(int id)
    {
        if (HttpContext.Session.GetString("Role") != "Admin")
        {
            TempData["Error"] = "Bu sayfaya erişim yetkiniz yok!";
            return RedirectToAction("Index", "Home");
        }

        var kullanici = await _context.Kullanicilar.FindAsync(id);
        if (kullanici != null)
        {
            // Kendi hesabını pasif yapma kontrolü
            var currentUserId = HttpContext.Session.GetString("KullaniciID");
            if (kullanici.KullaniciID.ToString() == currentUserId)
            {
                TempData["Error"] = "Kendi kullanıcı hesabınızı pasif yapamazsınız!";
                return RedirectToAction(nameof(Index));
            }

            kullanici.Aktif = !kullanici.Aktif;
            await _context.SaveChangesAsync();

            var durum = kullanici.Aktif ? "aktif" : "pasif";
            TempData["Success"] = $"{kullanici.AdSoyad} kullanıcısı {durum} yapıldı!";
        }
        return RedirectToAction(nameof(Index));
    }

    private bool KullaniciExists(int id)
    {
        return _context.Kullanicilar.Any(e => e.KullaniciID == id);
    }
}