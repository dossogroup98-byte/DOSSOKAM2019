using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("HazirMatbaa")]
public class HazirMatbaa
{
    [Key]
    [Column("HazirMatbaaID")]
    public int HazirMatbaaID { get; set; }

    [Required(ErrorMessage = "Sipariş No zorunlu")]
    [Column("SiparisNo")]
    public string SiparisNo { get; set; }

    [Required(ErrorMessage = "Ürün adı zorunlu")]
    [Column("UrunAdi")]
    public string UrunAdi { get; set; }

    [Column("Aciklama")]
    public string? Aciklama { get; set; }

    [Column("CikanAdet")]
    public int? CikanAdet { get; set; }

    [Column("KayitTarihi")]
    public DateTime KayitTarihi { get; set; } = DateTime.Now;

    [Column("KaydedenKullaniciID")]
    public int? KaydedenKullaniciID { get; set; }
}