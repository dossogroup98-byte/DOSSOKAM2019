using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOSSOKAM2019.Models;

[Table("HazirDokuma")]
public class HazirDokuma
{
    [Key]
    [Column("HazirDokumaID")]
    public int HazirDokumaID { get; set; }

    [Required(ErrorMessage = "Sipariş No zorunlu")]
    [Column("SiparisNo")]
    public string SiparisNo { get; set; }

    [Required(ErrorMessage = "Ürün adı zorunlu")]
    [Column("UrunAdi")]
    public string UrunAdi { get; set; }

    [Column("Aciklama")]
    public string? Aciklama { get; set; }

    [Column("BaslamaTarihi")]
    public DateTime? BaslamaTarihi { get; set; }

    [Column("BitisTarihi")]
    public DateTime? BitisTarihi { get; set; }

    [Column("GecenSure")]
    public decimal? GecenSure { get; set; } // Computed column - sadece okuma

    [Column("CikanAdet")]
    public int? CikanAdet { get; set; }

    [Column("Tamamlandi")]
    public bool Tamamlandi { get; set; } = false;

    [Column("KayitTarihi")]
    public DateTime KayitTarihi { get; set; } = DateTime.Now;
}