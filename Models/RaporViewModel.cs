using System;
using System.Collections.Generic;

namespace DOSSOKAM2019.Models;

public class RaporViewModel
{
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public string RaporTipi { get; set; }
    public DateTime OlusturmaTarihi { get; set; }

    public List<BolumRaporu> MakineSaatRapor { get; set; } = new List<BolumRaporu>();
    public List<BolumRaporu> EklemeRapor { get; set; } = new List<BolumRaporu>();
    public List<BolumRaporu> DazmalRapor { get; set; } = new List<BolumRaporu>();
    public List<BolumRaporu> KesmeRapor { get; set; } = new List<BolumRaporu>();
    public List<BolumRaporu> PaketlemeRapor { get; set; } = new List<BolumRaporu>();
    public List<BolumRaporu> HazirDokumaRapor { get; set; } = new List<BolumRaporu>();
    public List<BolumRaporu> HazirMatbaaRapor { get; set; } = new List<BolumRaporu>();
}

public class BolumRaporu
{
    public string UrunAdi { get; set; }
    public int Tamamlanan { get; set; }
    public int DevamEden { get; set; }
    public int Toplam { get; set; }
}