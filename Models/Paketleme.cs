using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOSSOKAM2019.Models;

[Table("Paketleme")]
public class Paketleme
{
    [Key]
    [Column("PaketlemeID")]
    public int PaketlemeID { get; set; }

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
    public decimal? GecenSure { get; set; }

    [Column("Tamamlandi")]
    public bool Tamamlandi { get; set; } = false;

    [Column("KayitTarihi")]
    public DateTime KayitTarihi { get; set; } = DateTime.Now;
}