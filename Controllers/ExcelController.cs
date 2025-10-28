using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Linq;
using System.Threading.Tasks;

public class ExcelController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExcelController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> ExportDashboard()
    {
        // YENİ LİSANS AYARI - EPPlus 8+ için
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Dashboard");

            // Başlık satırı
            worksheet.Cells[1, 1].Value = "Sipariş No";
            worksheet.Cells[1, 2].Value = "Ürün Adı";
            worksheet.Cells[1, 3].Value = "Bölüm";
            worksheet.Cells[1, 4].Value = "Durum";
            worksheet.Cells[1, 5].Value = "Başlama Tarihi";
            worksheet.Cells[1, 6].Value = "Kayıt Tarihi";

            // Stil
            using (var range = worksheet.Cells[1, 1, 1, 6])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }

            int row = 2;

            // Tüm bölümlerden verileri al
            var makineSaatData = await _context.MakineSaat
                .Select(x => new { x.SiparisNo, x.UrunAdi, Bolum = "Makine Saat", Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor", x.BaslamaTarihi, x.KayitTarihi })
                .ToListAsync();

            var eklemeData = await _context.Ekleme
                .Select(x => new { x.SiparisNo, x.UrunAdi, Bolum = "Ekleme", Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor", x.BaslamaTarihi, x.KayitTarihi })
                .ToListAsync();

            var dazmalData = await _context.Dazmal
                .Select(x => new { x.SiparisNo, x.UrunAdi, Bolum = "Dazmal", Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor", x.BaslamaTarihi, x.KayitTarihi })
                .ToListAsync();

            var kesmeData = await _context.Kesme
                .Select(x => new { x.SiparisNo, x.UrunAdi, Bolum = "Kesme", Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor", x.BaslamaTarihi, x.KayitTarihi })
                .ToListAsync();

            var paketlemeData = await _context.Kesme
                .Select(x => new { x.SiparisNo, x.UrunAdi, Bolum = "Paketleme", Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor", x.BaslamaTarihi, x.KayitTarihi })
                .ToListAsync();

            var hazirdokumaData = await _context.Kesme
                .Select(x => new { x.SiparisNo, x.UrunAdi, Bolum = "HazirDokuma", Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor", x.BaslamaTarihi, x.KayitTarihi })
                .ToListAsync();

            var hazirMatbaaData = await _context.Kesme
                .Select(x => new { x.SiparisNo, x.UrunAdi, Bolum = "HazirMatbaa", Durum = x.Tamamlandi ? "Tamamlandı" : "Devam Ediyor", x.BaslamaTarihi, x.KayitTarihi })
                .ToListAsync();



            // Verileri Excel'e yaz
            WriteToExcel(worksheet, ref row, makineSaatData);
            WriteToExcel(worksheet, ref row, eklemeData);
            WriteToExcel(worksheet, ref row, dazmalData);
            WriteToExcel(worksheet, ref row, kesmeData);
            WriteToExcel(worksheet, ref row, paketlemeData);
            WriteToExcel(worksheet, ref row, hazirdokumaData);
            WriteToExcel(worksheet, ref row, hazirMatbaaData);

            // Kolon genişliklerini ayarla
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            // Excel'i indir
            var fileName = $"Dashboard_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
            var fileBytes = package.GetAsByteArray();
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
    }

    private void WriteToExcel(ExcelWorksheet worksheet, ref int row, dynamic data)
    {
        foreach (var item in data)
        {
            worksheet.Cells[row, 1].Value = item.SiparisNo;
            worksheet.Cells[row, 2].Value = item.UrunAdi;
            worksheet.Cells[row, 3].Value = item.Bolum;
            worksheet.Cells[row, 4].Value = item.Durum;
            worksheet.Cells[row, 5].Value = item.BaslamaTarihi?.ToString("yyyy-MM-dd HH:mm");
            worksheet.Cells[row, 6].Value = item.KayitTarihi.ToString("yyyy-MM-dd HH:mm");
            row++;
        }
    }
}