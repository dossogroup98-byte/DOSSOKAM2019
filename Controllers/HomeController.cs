using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using DOSSOKAM2019.Services;
using DOSSOKAM2019.Models;
using Supabase;

namespace DOSSOKAM2019.Controllers
{
    public class HomeController : Controller
    {
        private readonly SupabaseService _supabase;

        public HomeController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public async Task<IActionResult> Index()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("UserEmail")))
                return RedirectToAction("Login", "Account");

            var rol = HttpContext.Session.GetString("Rol");
            ViewBag.Rol = rol;

            var client = _supabase.Client;

            // Aktif (tamamlanmamış) kayıtlar
            var makineSaat = await client.From<MakineSaat>("makine_saat")
                .Where(x => x.Tamamlandi == false).Get();
            var ekleme = await client.From<Ekleme>("ekleme")
                .Where(x => x.Tamamlandi == false).Get();
            var dazmal = await client.From<Dazmal>("dazmal")
                .Where(x => x.Tamamlandi == false).Get();
            var kesme = await client.From<Kesme>("kesme")
                .Where(x => x.Tamamlandi == false).Get();
            var paketleme = await client.From<Paketleme>("paketleme")
                .Where(x => x.Tamamlandi == false).Get();

            ViewBag.MakineSaat = makineSaat.Models;
            ViewBag.Ekleme = ekleme.Models;
            ViewBag.Dazmal = dazmal.Models;
            ViewBag.Kesme = kesme.Models;
            ViewBag.Paketleme = paketleme.Models;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AsamaBitir(string siparisNo, string asama)
        {
            var client = _supabase.Client;

            if (asama == "MakineSaat")
            {
                var ms = (await client.From<MakineSaat>("makine_saat")
                    .Where(x => x.SiparisNo == siparisNo).Get()).Models.First();
                ms.Tamamlandi = true;
                ms.BitisTarihi = DateTime.UtcNow;
                await client.From<MakineSaat>("makine_saat").Update(ms);

                await client.From<Ekleme>("ekleme").Insert(new Ekleme
                {
                    SiparisNo = siparisNo,
                    UrunAdi = ms.UrunAdi,
                    BaslamaTarihi = DateTime.UtcNow
                });
            }
            // Diğer aşamalar benzer şekilde eklenebilir

            return RedirectToAction("Index");
        }
    }
}