using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DOSSOKAM2019.Models;

[Table("MakineSaat")]
public class MakineSaat
{
    [Key]
    [Column("MakineSaatID")]
    public int MakineSaatID { get; set; }

    [Required(ErrorMessage = "Sipariş No zorunlu")]
    [Column("SiparisNo")]
    public string SiparisNo { get; set; }

    [Required(ErrorMessage = "Ürün adı zorunlu")]
    [Column("UrunAdi")]
    public string UrunAdi { get; set; }

    [Range(0.1, double.MaxValue, ErrorMessage = "Kalan saat 0'dan büyük olmalı")]
    [Column("KalanSaat")]
    public decimal KalanSaat { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Miktar 1'den büyük olmalı")]
    [Column("SiparisMiktar")]
    public int SiparisMiktar { get; set; }

    [Column("Aciklama")]
    public string? Aciklama { get; set; }

    [Column("Dokumaci1ID")]
    public int? Dokumaci1ID { get; set; }

    [Column("Dokumaci2ID")]
    public int? Dokumaci2ID { get; set; }

    [Column("BaslamaTarihi")]
    public DateTime BaslamaTarihi { get; set; } = DateTime.Now;

    [Column("BitisTarihi")]
    public DateTime? BitisTarihi { get; set; }

    [Column("GecenSure")]
    public decimal? GecenSure { get; set; }

    [Column("Tamamlandi")]
    public bool Tamamlandi { get; set; } = false;

    [Column("KayitTarihi")]
    public DateTime KayitTarihi { get; set; } = DateTime.Now;
}