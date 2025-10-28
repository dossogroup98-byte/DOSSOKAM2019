using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using DOSSOKAM2019.Services;
using DOSSOKAM2019.Models;

namespace DOSSOKAM2019.Controllers
{
    public class AccountController : Controller
    {
        private readonly SupabaseService _supabase;

        public AccountController(SupabaseService supabase)
        {
            _supabase = supabase;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var session = await _supabase.Client.Auth.SignIn(email, password);
                HttpContext.Session.SetString("AccessToken", session.AccessToken);
                HttpContext.Session.SetString("UserEmail", email);

                string rol = "Vizor";
                if (email.Contains("admin")) rol = "Admin";
                else if (email.Contains("personel")) rol = "Personel";

                HttpContext.Session.SetString("Rol", rol);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ViewBag.Hata = "Giriş başarısız: " + ex.Message;
                return View();
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}