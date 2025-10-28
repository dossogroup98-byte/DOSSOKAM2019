using System.ComponentModel.DataAnnotations.Schema;

[Table("Kullanicilar")]
public class Kullanici
{
    [Column("KullaniciID")]
    public int KullaniciID { get; set; }

    [Column("KullaniciAdi")]
    public string KullaniciAdi { get; set; }

    [Column("Sifre")]
    public string Sifre { get; set; }

    // KULLANICITIPI KALDIRILDI

    [Column("AdSoyad")]
    public string AdSoyad { get; set; }

    [Column("Email")]
    public string? Email { get; set; }

    [Column("Aktif")]
    public bool Aktif { get; set; } = true;

    [Column("KayitTarihi")]
    public DateTime KayitTarihi { get; set; } = DateTime.Now;

    [Column("Role")]
    public string Role { get; set; } = "Personel";
}