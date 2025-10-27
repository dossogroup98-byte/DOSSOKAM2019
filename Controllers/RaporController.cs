using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Linq;
using System.Threading.Tasks;

public class RaporController : Controller
{
    private readonly ApplicationDbContext _context;

    public RaporController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Rapor Sayfası
    public IActionResult Index()
    {
        return View();
    }

    // Haftalık Rapor
    public async Task<IActionResult> HaftalikRapor()
    {
        var baslangic = DateTime.Today.AddDays(-7);
        var bitis = DateTime.Today;

        var rapor = await RaporVerileriniGetir(baslangic, bitis, "Haftalık");
        return View("RaporView", rapor);
    }

    // Aylık Rapor
    public async Task<IActionResult> AylikRapor()
    {
        var baslangic = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        var bitis = baslangic.AddMonths(1).AddDays(-1);

        var rapor = await RaporVerileriniGetir(baslangic, bitis, "Aylık");
        return View("RaporView", rapor);
    }

    // Özel Tarih Aralığı Raporu
    [HttpPost]
    public async Task<IActionResult> OzelRapor(DateTime baslangicTarihi, DateTime bitisTarihi)
    {
        var rapor = await RaporVerileriniGetir(baslangicTarihi, bitisTarihi, "Özel");
        return View("RaporView", rapor);
    }

    private async Task<RaporViewModel> RaporVerileriniGetir(DateTime baslangic, DateTime bitis, string raporTipi)
    {
        var rapor = new RaporViewModel
        {
            BaslangicTarihi = baslangic,
            BitisTarihi = bitis,
            RaporTipi = raporTipi,
            OlusturmaTarihi = DateTime.Now
        };

        // Makine Saat Raporu
        rapor.MakineSaatRapor = await _context.MakineSaat
            .Where(x => x.KayitTarihi >= baslangic && x.KayitTarihi <= bitis)
            .GroupBy(x => x.UrunAdi)
            .Select(g => new BolumRaporu
            {
                UrunAdi = g.Key,
                Tamamlanan = g.Count(x => x.Tamamlandi),
                DevamEden = g.Count(x => !x.Tamamlandi),
                Toplam = g.Count()
            })
            .ToListAsync();

        // Ekleme Raporu
        rapor.EklemeRapor = await _context.Ekleme
            .Where(x => x.KayitTarihi >= baslangic && x.KayitTarihi <= bitis)
            .GroupBy(x => x.UrunAdi)
            .Select(g => new BolumRaporu
            {
                UrunAdi = g.Key,
                Tamamlanan = g.Count(x => x.Tamamlandi),
                DevamEden = g.Count(x => !x.Tamamlandi),
                Toplam = g.Count()
            })
            .ToListAsync();

        // Dazmal Raporu
        rapor.DazmalRapor = await _context.Dazmal
            .Where(x => x.KayitTarihi >= baslangic && x.KayitTarihi <= bitis)
            .GroupBy(x => x.UrunAdi)
            .Select(g => new BolumRaporu
            {
                UrunAdi = g.Key,
                Tamamlanan = g.Count(x => x.Tamamlandi),
                DevamEden = g.Count(x => !x.Tamamlandi),
                Toplam = g.Count()
            })
            .ToListAsync();

        // Kesme Raporu
        rapor.KesmeRapor = await _context.Kesme
            .Where(x => x.KayitTarihi >= baslangic && x.KayitTarihi <= bitis)
            .GroupBy(x => x.UrunAdi)
            .Select(g => new BolumRaporu
            {
                UrunAdi = g.Key,
                Tamamlanan = g.Count(x => x.Tamamlandi),
                DevamEden = g.Count(x => !x.Tamamlandi),
                Toplam = g.Count()
            })
            .ToListAsync();

        // Paketleme Raporu
        rapor.PaketlemeRapor = await _context.Paketleme
            .Where(x => x.KayitTarihi >= baslangic && x.KayitTarihi <= bitis)
            .GroupBy(x => x.UrunAdi)
            .Select(g => new BolumRaporu
            {
                UrunAdi = g.Key,
                Tamamlanan = 0, // Bu bölümde Tamamlandi property'si yok
                DevamEden = g.Count(),
                Toplam = g.Count()
            })
            .ToListAsync();

        // Hazır Dokuma Raporu
        rapor.HazirDokumaRapor = await _context.HazirDokuma
            .Where(x => x.KayitTarihi >= baslangic && x.KayitTarihi <= bitis)
            .GroupBy(x => x.UrunAdi)
            .Select(g => new BolumRaporu
            {
                UrunAdi = g.Key,
                Tamamlanan = 0, // Bu bölümde Tamamlandi property'si yok
                DevamEden = g.Count(),
                Toplam = g.Count()
            })
            .ToListAsync();

        // Hazır Matbaa Raporu
        rapor.HazirMatbaaRapor = await _context.HazirMatbaa
            .Where(x => x.KayitTarihi >= baslangic && x.KayitTarihi <= bitis)
            .GroupBy(x => x.UrunAdi)
            .Select(g => new BolumRaporu
            {
                UrunAdi = g.Key,
                Tamamlanan = 0, // Bu bölümde Tamamlandi property'si yok
                DevamEden = g.Count(),
                Toplam = g.Count()
            })
            .ToListAsync();

        return rapor;
    }

    // Excel'e Aktar
    public async Task<IActionResult> ExcelAktar(string raporTipi, DateTime baslangic, DateTime bitis)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        var rapor = await RaporVerileriniGetir(baslangic, bitis, raporTipi);

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add($"{raporTipi}_Rapor");

            // Başlık
            worksheet.Cells[1, 1].Value = $"{raporTipi} Üretim Raporu";
            worksheet.Cells[1, 1].Style.Font.Bold = true;
            worksheet.Cells[1, 1].Style.Font.Size = 16;

            worksheet.Cells[2, 1].Value = $"Tarih Aralığı: {baslangic:dd.MM.yyyy} - {bitis:dd.MM.yyyy}";
            worksheet.Cells[3, 1].Value = $"Oluşturulma: {DateTime.Now:dd.MM.yyyy HH:mm}";

            // Makine Saat Raporu
            worksheet.Cells[5, 1].Value = "MAKİNE SAAT RAPORU";
            worksheet.Cells[5, 1].Style.Font.Bold = true;
            worksheet.Cells[6, 1].Value = "Ürün Adı";
            worksheet.Cells[6, 2].Value = "Tamamlanan";
            worksheet.Cells[6, 3].Value = "Devam Eden";
            worksheet.Cells[6, 4].Value = "Toplam";

            int row = 7;
            foreach (var item in rapor.MakineSaatRapor)
            {
                worksheet.Cells[row, 1].Value = item.UrunAdi;
                worksheet.Cells[row, 2].Value = item.Tamamlanan;
                worksheet.Cells[row, 3].Value = item.DevamEden;
                worksheet.Cells[row, 4].Value = item.Toplam;
                row++;
            }

            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            var fileName = $"{raporTipi}_Rapor_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            return File(package.GetAsByteArray(),
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       fileName);
        }
    }
}