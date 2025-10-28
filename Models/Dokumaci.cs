using System.ComponentModel.DataAnnotations.Schema;

namespace DOSSOKAM2019.Models;

[Table("Dokumacilar")]
public class Dokumaci
{
    [Column("DokumaciID")]
    public int DokumaciID { get; set; }

    [Column("DokumaciAdi")]
    public string DokumaciAdi { get; set; }

    [Column("Telefon")]
    public string? Telefon { get; set; }

    [Column("Adres")]
    public string? Adres { get; set; }

    [Column("Aktif")]
    public bool Aktif { get; set; } = true;

    [Column("KayitTarihi")]
    public DateTime KayitTarihi { get; set; } = DateTime.Now;
}