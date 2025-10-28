public class DashboardViewModel
{
    public int ToplamMakineSaat { get; set; }
    public int ToplamEkleme { get; set; }
    public int ToplamDazmal { get; set; }
    public int ToplamKesme { get; set; }
    public int ToplamPaketleme { get; set; }
    public int ToplamHazirDokuma { get; set; }
    public int ToplamHazirMatbaa { get; set; }

    public List<MakineSaat> SonMakineSaatler { get; set; }
    public List<Ekleme> SonEklemeIsleri { get; set; }
    public List<UrunDurumu> TumUrunDurumlari { get; set; }
}

public class UrunDurumu
{
    public string SiparisNo { get; set; }
    public string UrunAdi { get; set; }
    public string MevcutBolum { get; set; }
    public string Durum { get; set; }
    public DateTime? BaslamaTarihi { get; set; }
    public string RenkKodu { get; set; }
}