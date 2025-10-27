using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly ApplicationDbContext _context;

    public HomeController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string search = "")
    {
        if (string.IsNullOrEmpty(HttpContext.Session.GetString("KullaniciAdi")))
            return RedirectToAction("Login", "Account");

        var dashboard = new DashboardViewModel();

        try
        {
            // İstatistikler - TÜM BÖLÜMLER
            dashboard.ToplamMakineSaat = await _context.MakineSaat.CountAsync();
            dashboard.ToplamEkleme = await _context.Ekleme.CountAsync();
            dashboard.ToplamDazmal = await _context.Dazmal.CountAsync();
            dashboard.ToplamKesme = await _context.Kesme.CountAsync();
            dashboard.ToplamPaketleme = await _context.Paketleme.CountAsync();
            dashboard.ToplamHazirDokuma = await _context.HazirDokuma.CountAsync();
            dashboard.ToplamHazirMatbaa = await _context.HazirMatbaa.CountAsync();

            // Son kayıtlar - TÜM KAYITLAR
            dashboard.SonMakineSaatler = await _context.MakineSaat
                .OrderByDescending(x => x.KayitTarihi)
                .Take(5)
                .ToListAsync();

            dashboard.SonEklemeIsleri = await _context.Ekleme
                .OrderByDescending(x => x.KayitTarihi)
                .Take(5)
                .ToListAsync();

            // Tüm ürün durumları - ARAMA FİLTRESİ EKLENDİ
            await TumUrunDurumlariniDoldur(dashboard, search);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Dashboard yüklenirken hata: {ex.Message}";
        }

        return View(dashboard);
    }

    private async Task TumUrunDurumlariniDoldur(DashboardViewModel dashboard, string search)
    {
        // ARAMA FİLTRESİ - SiparişNo veya UrunAdi'na göre
        var makineSaatUrunler = await _context.MakineSaat
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Makine Saat",
                Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor",
                BaslamaTarihi = x.BaslamaTarihi,
                RenkKodu = x.Tamamlandi ? "success" : "danger"
            })
            .ToListAsync();

        var eklemeUrunler = await _context.Ekleme
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Ekleme",
                Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor",
                BaslamaTarihi = x.BaslamaTarihi,
                RenkKodu = x.Tamamlandi ? "success" : "warning"
            })
            .ToListAsync();

        var dazmalUrunler = await _context.Dazmal
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Dazmal",
                Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor",
                BaslamaTarihi = x.BaslamaTarihi,
                RenkKodu = x.Tamamlandi ? "success" : "info"
            })
            .ToListAsync();

        var kesmeUrunler = await _context.Kesme
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Kesme",
                Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor",
                BaslamaTarihi = x.BaslamaTarihi,
                RenkKodu = x.Tamamlandi ? "success" : "primary"
            })
            .ToListAsync();

        // Paketleme - SADECE MEVCUT PROPERTY'LERİ KULLAN
        var paketlemeUrunler = await _context.Paketleme
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Paketleme",
                Durum = "Devam Ediyor",
                BaslamaTarihi = DateTime.Now,
                RenkKodu = "secondary"
            })
            .ToListAsync();

        // Hazır Dokuma - SADECE MEVCUT PROPERTY'LERİ KULLAN
        var hazirDokumaUrunler = await _context.HazirDokuma
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Hazır Dokuma",
                Durum = "Devam Ediyor",
                BaslamaTarihi = DateTime.Now,
                RenkKodu = "dark"
            })
            .ToListAsync();

        // Hazır Matbaa - SADECE MEVCUT PROPERTY'LERİ KULLAN
        var hazirMatbaaUrunler = await _context.HazirMatbaa
            .Where(x => string.IsNullOrEmpty(search) ||
                       x.SiparisNo.Contains(search) ||
                       x.UrunAdi.Contains(search))
            .Select(x => new UrunDurumu
            {
                SiparisNo = x.SiparisNo,
                UrunAdi = x.UrunAdi,
                MevcutBolum = "Hazır Matbaa",
                Durum = "Devam Ediyor",
                BaslamaTarihi = DateTime.Now,
                RenkKodu = "light"
            })
            .ToListAsync();

        // Tümünü birleştir
        dashboard.TumUrunDurumlari = makineSaatUrunler
            .Concat(eklemeUrunler)
            .Concat(dazmalUrunler)
            .Concat(kesmeUrunler)
            .Concat(paketlemeUrunler)
            .Concat(hazirDokumaUrunler)
            .Concat(hazirMatbaaUrunler)
            .OrderByDescending(x => x.BaslamaTarihi)
            .ToList();
    }
}

//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//public class HomeController : Controller
//{
//    private readonly ApplicationDbContext _context;

//    public HomeController(ApplicationDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<IActionResult> Index()
//    {
//        if (string.IsNullOrEmpty(HttpContext.Session.GetString("KullaniciAdi")))
//            return RedirectToAction("Login", "Account");

//        var dashboard = new DashboardViewModel();

//        try
//        {
//            // İstatistikler - TÜM BÖLÜMLER
//            dashboard.ToplamMakineSaat = await _context.MakineSaat.CountAsync();
//            dashboard.ToplamEkleme = await _context.Ekleme.CountAsync();
//            dashboard.ToplamDazmal = await _context.Dazmal.CountAsync();
//            dashboard.ToplamKesme = await _context.Kesme.CountAsync();
//            dashboard.ToplamPaketleme = await _context.Paketleme.CountAsync();
//            dashboard.ToplamHazirDokuma = await _context.HazirDokuma.CountAsync();
//            dashboard.ToplamHazirMatbaa = await _context.HazirMatbaa.CountAsync();

//            // Son kayıtlar - TÜM KAYITLAR
//            dashboard.SonMakineSaatler = await _context.MakineSaat
//                .OrderByDescending(x => x.KayitTarihi)
//                .Take(5)
//                .ToListAsync();

//            dashboard.SonEklemeIsleri = await _context.Ekleme
//                .OrderByDescending(x => x.KayitTarihi)
//                .Take(5)
//                .ToListAsync();

//            // Tüm ürün durumları - TÜM BÖLÜMLER
//            await TumUrunDurumlariniDoldur(dashboard);
//        }
//        catch (Exception ex)
//        {
//            TempData["Error"] = $"Dashboard yüklenirken hata: {ex.Message}";
//        }

//        return View(dashboard);
//    }

//    private async Task TumUrunDurumlariniDoldur(DashboardViewModel dashboard)
//    {
//        // TÜM BÖLÜMLERDEN verileri al
//        var makineSaatUrunler = await _context.MakineSaat
//            .Select(x => new UrunDurumu
//            {
//                SiparisNo = x.SiparisNo,
//                UrunAdi = x.UrunAdi,
//                MevcutBolum = "Makine Saat",
//                Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor",
//                BaslamaTarihi = x.BaslamaTarihi,
//                RenkKodu = x.Tamamlandi ? "success" : "danger"
//            })
//            .ToListAsync();

//        var eklemeUrunler = await _context.Ekleme
//            .Select(x => new UrunDurumu
//            {
//                SiparisNo = x.SiparisNo,
//                UrunAdi = x.UrunAdi,
//                MevcutBolum = "Ekleme",
//                Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor",
//                BaslamaTarihi = x.BaslamaTarihi,
//                RenkKodu = x.Tamamlandi ? "success" : "warning"
//            })
//            .ToListAsync();

//        var dazmalUrunler = await _context.Dazmal
//            .Select(x => new UrunDurumu
//            {
//                SiparisNo = x.SiparisNo,
//                UrunAdi = x.UrunAdi,
//                MevcutBolum = "Dazmal",
//                Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor",
//                BaslamaTarihi = x.BaslamaTarihi,
//                RenkKodu = x.Tamamlandi ? "success" : "info"
//            })
//            .ToListAsync();

//        var kesmeUrunler = await _context.Kesme
//            .Select(x => new UrunDurumu
//            {
//                SiparisNo = x.SiparisNo,
//                UrunAdi = x.UrunAdi,
//                MevcutBolum = "Kesme",
//                Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor",
//                BaslamaTarihi = x.BaslamaTarihi,
//                RenkKodu = x.Tamamlandi ? "success" : "primary"
//            })
//            .ToListAsync();

//        // Paketleme - SADECE MEVCUT PROPERTY'LERİ KULLAN
//        var paketlemeUrunler = await _context.Paketleme
//            .Select(x => new UrunDurumu
//            {
//                SiparisNo = x.SiparisNo,
//                UrunAdi = x.UrunAdi,
//                MevcutBolum = "Paketleme",
//                Durum = "Devam Ediyor", // Varsayılan değer
//                BaslamaTarihi = DateTime.Now, // Varsayılan değer
//                RenkKodu = "secondary"
//            })
//            .ToListAsync();

//        // Hazır Dokuma - SADECE MEVCUT PROPERTY'LERİ KULLAN
//        var hazirDokumaUrunler = await _context.HazirDokuma
//            .Select(x => new UrunDurumu
//            {
//                SiparisNo = x.SiparisNo,
//                UrunAdi = x.UrunAdi,
//                MevcutBolum = "Hazır Dokuma",
//                Durum = "Devam Ediyor",
//                BaslamaTarihi = DateTime.Now,
//                RenkKodu = "dark"
//            })
//            .ToListAsync();

//        // Hazır Matbaa - SADECE MEVCUT PROPERTY'LERİ KULLAN
//        var hazirMatbaaUrunler = await _context.HazirMatbaa
//            .Select(x => new UrunDurumu
//            {
//                SiparisNo = x.SiparisNo,
//                UrunAdi = x.UrunAdi,
//                MevcutBolum = "Hazır Matbaa",
//                Durum = "Devam Ediyor",
//                BaslamaTarihi = DateTime.Now,
//                RenkKodu = "light"
//            })
//            .ToListAsync();

//        // Tümünü birleştir
//        dashboard.TumUrunDurumlari = makineSaatUrunler
//            .Concat(eklemeUrunler)
//            .Concat(dazmalUrunler)
//            .Concat(kesmeUrunler)
//            .Concat(paketlemeUrunler)
//            .Concat(hazirDokumaUrunler)
//            .Concat(hazirMatbaaUrunler)
//            .OrderByDescending(x => x.BaslamaTarihi)
//            .ToList();
//    }
//}