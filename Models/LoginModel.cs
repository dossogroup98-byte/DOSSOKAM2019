using System.ComponentModel.DataAnnotations;

public class LoginModel
{
    [Required(ErrorMessage = "Kullanıcı adı zorunlu")]
    public string KullaniciAdi { get; set; }

    [Required(ErrorMessage = "Şifre zorunlu")]
    public string Sifre { get; set; }

    public bool BeniHatirla { get; set; }
}