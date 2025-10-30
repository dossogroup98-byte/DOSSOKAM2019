using Dapper;
using Npgsql;
using OfficeOpenXml;
using System.ComponentModel;
using System.Text;

// Render PORT desteği
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ✅ EPPLUS 8+ LİSANS AYARI - TAM NAMESPACE İLE
OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

var builder = WebApplication.CreateBuilder(args);

// CORS
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()));

// ✅ POSTGRESQL BAĞLANTISI
builder.Services.AddScoped<NpgsqlConnection>(serviceProvider =>
{
    var connectionString = "Host=dpg-d40ugp75r7bs7388bsrg-a.oregon-postgres.render.com;" +
                          "Port=5432;" +
                          "Database=dokuma_takip;" +
                          "Username=dokuma_user;" +
                          "Password=Pm41PLYnjRqukLgaUL2vetPrkZxiBOrq;" +
                          "SSL Mode=Require;Trust Server Certificate=true;";
    return new NpgsqlConnection(connectionString);
});

var app = builder.Build();
app.UseCors("AllowAll");

// ✅ ANA SAYFA


// ✅ POSTGRESQL TEST
app.MapGet("/api/test-db", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();
        var result = await db.QueryFirstOrDefaultAsync<string>("SELECT 'PostgreSQL bağlantısı başarılı!' AS test_message");
        return Results.Ok(new { success = true, message = result });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Database hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ TABLOLARI LİSTELE
app.MapGet("/api/list-tables", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();
        var tables = await db.QueryAsync<string>(
            "SELECT table_name FROM information_schema.tables WHERE table_schema = 'public'"
        );
        return Results.Ok(new { success = true, tables = tables.ToList() });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ TÜM TABLOLARI OLUŞTUR
app.MapGet("/api/create-all-tables", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();

        // 1. KULLANICILAR TABLOSU
        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS kullanicilar (
                id SERIAL PRIMARY KEY,
                kullanici_id VARCHAR(50) UNIQUE NOT NULL,
                kullanici_adi VARCHAR(100) UNIQUE NOT NULL,
                sifre VARCHAR(255) NOT NULL,
                kullanici_tipi VARCHAR(20) CHECK (kullanici_tipi IN ('admin', 'operator', 'viewer')),
                ad_soyad VARCHAR(200) NOT NULL,
                email VARCHAR(200),
                aktif BOOLEAN DEFAULT true,
                kayit_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )");

        // 2. DOKUMACILAR TABLOSU
        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS dokumacilar (
                id SERIAL PRIMARY KEY,
                dokumaci_id VARCHAR(50) UNIQUE NOT NULL,
                dokumaci_adi VARCHAR(200) NOT NULL,
                telefon VARCHAR(20),
                adres TEXT,
                aktif BOOLEAN DEFAULT true,
                kayit_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )");

        // 3. MAKINE SAAT TABLOSU
        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS makinesaat (
                id SERIAL PRIMARY KEY,
                makinesaat_id VARCHAR(50) UNIQUE NOT NULL,
                makine_no VARCHAR(20) CHECK (makine_no IN ('Makine1','Makine2','Makine3','Makine4','Makine5','Makine6','Makine7','Makine8','Makine9','Makine10')),
                siparis_no VARCHAR(100) NOT NULL,
                urun_adi VARCHAR(200) NOT NULL,
                kalan_saat INTEGER DEFAULT 0,
                siparis_miktar INTEGER NOT NULL,
                aciklama TEXT,
                dokumaci1_id VARCHAR(50),
                dokumaci2_id VARCHAR(50),
                baslama_tarihi TIMESTAMP,
                bitis_tarihi TIMESTAMP,
                gecen_sure INTEGER DEFAULT 0,
                tamamlandi BOOLEAN DEFAULT false,
                kayit_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )");

        // 4. EKLEME TABLOSU
        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS ekleme (
                id SERIAL PRIMARY KEY,
                ekleme_id VARCHAR(50) UNIQUE NOT NULL,
                siparis_no VARCHAR(100) NOT NULL,
                urun_adi VARCHAR(200) NOT NULL,
                aciklama TEXT,
                baslama_tarihi TIMESTAMP,
                bitis_tarihi TIMESTAMP,
                gecen_sure INTEGER DEFAULT 0,
                tamamlandi BOOLEAN DEFAULT false,
                kayit_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )");

        // 5. DAZMAL TABLOSU
        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS dazmal (
                id SERIAL PRIMARY KEY,
                dazmal_id VARCHAR(50) UNIQUE NOT NULL,
                siparis_no VARCHAR(100) NOT NULL,
                urun_adi VARCHAR(200) NOT NULL,
                aciklama TEXT,
                baslama_tarihi TIMESTAMP,
                bitis_tarihi TIMESTAMP,
                gecen_sure INTEGER DEFAULT 0,
                tamamlandi BOOLEAN DEFAULT false,
                kayit_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )");

        // 6. KESME TABLOSU
        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS kesme (
                id SERIAL PRIMARY KEY,
                kesme_id VARCHAR(50) UNIQUE NOT NULL,
                siparis_no VARCHAR(100) NOT NULL,
                urun_adi VARCHAR(200) NOT NULL,
                aciklama TEXT,
                baslama_tarihi TIMESTAMP,
                bitis_tarihi TIMESTAMP,
                gecen_sure INTEGER DEFAULT 0,
                tamamlandi BOOLEAN DEFAULT false,
                kayit_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )");

        // 7. PAKETLEME TABLOSU
        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS paketleme (
                id SERIAL PRIMARY KEY,
                paketleme_id VARCHAR(50) UNIQUE NOT NULL,
                siparis_no VARCHAR(100) NOT NULL,
                urun_adi VARCHAR(200) NOT NULL,
                aciklama TEXT,
                baslama_tarihi TIMESTAMP,
                bitis_tarihi TIMESTAMP,
                gecen_sure INTEGER DEFAULT 0,
                tamamlandi BOOLEAN DEFAULT false,
                kayit_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )");

        // 8. HAZIR DOKUMA TABLOSU
        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS hazirdokuma (
                id SERIAL PRIMARY KEY,
                hazirdokuma_id VARCHAR(50) UNIQUE NOT NULL,
                siparis_no VARCHAR(100) NOT NULL,
                urun_adi VARCHAR(200) NOT NULL,
                aciklama TEXT,
                baslama_tarihi TIMESTAMP,
                bitis_tarihi TIMESTAMP,
                gecen_sure INTEGER DEFAULT 0,
                tamamlandi BOOLEAN DEFAULT false,
                kayit_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP
            )");

        // 9. HAZIR MATBAA TABLOSU
        await db.ExecuteAsync(@"
            CREATE TABLE IF NOT EXISTS hazirmatbaa (
                id SERIAL PRIMARY KEY,
                hazirmatbaa_id VARCHAR(50) UNIQUE NOT NULL,
                siparis_no VARCHAR(100) NOT NULL,
                urun_adi VARCHAR(200) NOT NULL,
                aciklama TEXT,
                kayit_tarihi TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                kaydeden_kullanici_id VARCHAR(50)
            )");

        return Results.Ok(new { success = true, message = "TÜM tablolar oluşturuldu! Artık API'leri yazabiliriz." });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Tablo oluşturma hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ TEST KULLANICI EKLE
app.MapGet("/api/create-test-user", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();

        // Test kullanıcısı ekle (eğer yoksa)
        await db.ExecuteAsync(@"
            INSERT INTO kullanicilar (kullanici_id, kullanici_adi, sifre, kullanici_tipi, ad_soyad, email)
            VALUES ('KUL001', 'admin', '1234', 'admin', 'Sistem Yöneticisi', 'admin@dossokam.com')
            ON CONFLICT (kullanici_adi) DO NOTHING");

        return Results.Ok(new { success = true, message = "Test kullanıcısı hazır: admin / 1234" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ KULLANICI GİRİŞ
app.MapPost("/api/login", async (NpgsqlConnection db, LoginRequest request) =>
{
    try
    {
        await db.OpenAsync();
        var user = await db.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT kullanici_id, kullanici_adi, kullanici_tipi, ad_soyad FROM kullanicilar WHERE kullanici_adi = @Username AND sifre = @Password AND aktif = true",
            new { request.Username, request.Password });

        if (user != null)
            return Results.Ok(new
            {
                success = true,
                user = new
                {
                    user.kullanici_id,
                    user.kullanici_adi,
                    user.kullanici_tipi,
                    user.ad_soyad
                }
            });
        else
            return Results.Ok(new { success = false, message = "Kullanıcı adı veya şifre hatalı!" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Giriş hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ DASHBOARD VERİLERİ
app.MapGet("/api/dashboard", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();

        // Aktif makine sayısı
        var aktifMakineler = await db.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(*) FROM makinesaat WHERE tamamlandi = false");

        // Tamamlanan işler
        var tamamlananIsler = await db.QueryFirstOrDefaultAsync<int>(
            "SELECT COUNT(*) FROM makinesaat WHERE tamamlandi = true");

        // Son eklenen makine saatleri
        var sonMakineSaatleri = await db.QueryAsync<dynamic>(
            "SELECT makine_no, siparis_no, urun_adi, kalan_saat, tamamlandi FROM makinesaat ORDER BY kayit_tarihi DESC LIMIT 5");

        // Bölümlere göre iş dağılımı
        var bolumDagilimi = await db.QueryAsync<dynamic>(@"
            SELECT 'MakineSaat' as bolum, COUNT(*) as sayi FROM makinesaat WHERE tamamlandi = false
            UNION ALL SELECT 'Ekleme', COUNT(*) FROM ekleme WHERE tamamlandi = false
            UNION ALL SELECT 'Dazmal', COUNT(*) FROM dazmal WHERE tamamlandi = false
            UNION ALL SELECT 'Kesme', COUNT(*) FROM kesme WHERE tamamlandi = false
            UNION ALL SELECT 'Paketleme', COUNT(*) FROM paketleme WHERE tamamlandi = false
        ");

        return Results.Ok(new
        {
            aktifMakineler,
            tamamlananIsler,
            sonMakineSaatleri = sonMakineSaatleri.ToList(),
            bolumDagilimi = bolumDagilimi.ToList()
        });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Dashboard hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ MAKINE SAAT LİSTELE
app.MapGet("/api/makinesaat", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();
        var makineSaatleri = await db.QueryAsync<dynamic>(
            "SELECT * FROM makinesaat ORDER BY kayit_tarihi DESC");
        return Results.Ok(makineSaatleri);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ MAKINE SAAT EKLE
app.MapPost("/api/makinesaat", async (NpgsqlConnection db, MakineSaat model) =>
{
    try
    {
        await db.OpenAsync();

        // Yeni ID oluştur
        model.makinesaat_id = "MS" + DateTime.Now.Ticks;
        model.kayit_tarihi = DateTime.Now;

        await db.ExecuteAsync(@"
            INSERT INTO makinesaat (makinesaat_id, makine_no, siparis_no, urun_adi, kalan_saat, siparis_miktar, aciklama, dokumaci1_id, dokumaci2_id, baslama_tarihi, kayit_tarihi)
            VALUES (@makinesaat_id, @makine_no, @siparis_no, @urun_adi, @kalan_saat, @siparis_miktar, @aciklama, @dokumaci1_id, @dokumaci2_id, @baslama_tarihi, @kayit_tarihi)",
            model);

        return Results.Ok(new { success = true, message = "Makine saati eklendi!", id = model.makinesaat_id });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ekleme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ MAKINE SAAT SİL
app.MapDelete("/api/makinesaat/{id}", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();
        var affectedRows = await db.ExecuteAsync("DELETE FROM makinesaat WHERE makinesaat_id = @id", new { id });

        if (affectedRows > 0)
            return Results.Ok(new { success = true, message = "Makine saati silindi!" });
        else
            return Results.NotFound(new { success = false, message = "Makine saati bulunamadı!" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Silme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ MAKINE SAAT COMPLETE (ÖNCEKİ KAYDI SİLEREK)
app.MapPost("/api/makinesaat/{id}/complete", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();

        // Makine saat bilgilerini al
        var makine = await db.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT siparis_no, urun_adi, baslama_tarihi FROM makinesaat WHERE makinesaat_id = @id",
            new { id });

        if (makine == null)
            return Results.NotFound(new { success = false, message = "Makine saati bulunamadı!" });

        var bitisTarihi = DateTime.Now;

        // 1. ÖNCE ekleme bölümüne yeni kayıt oluştur
        var eklemeId = "EKM" + DateTime.Now.Ticks;
        await db.ExecuteAsync(@"
            INSERT INTO ekleme (ekleme_id, siparis_no, urun_adi, baslama_tarihi, aciklama)
            VALUES (@eklemeId, @siparis_no, @urun_adi, @baslamaTarihi, @aciklama)",
            new
            {
                eklemeId,
                makine.siparis_no,
                makine.urun_adi,
                baslamaTarihi = bitisTarihi,
                aciklama = $"Makine saatinden otomatik geçiş - {id}"
            });

        // 2. SONRA makine saat kaydını sil
        var affectedRows = await db.ExecuteAsync("DELETE FROM makinesaat WHERE makinesaat_id = @id", new { id });

        if (affectedRows > 0)
        {
            return Results.Ok(new
            {
                success = true,
                message = "Makine saati tamamlandı ve ekleme bölümüne geçildi!",
                yeni_ekleme_id = eklemeId
            });
        }
        else
        {
            return Results.NotFound(new { success = false, message = "Makine saati bulunamadı!" });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Complete hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});


// ✅ DAZMAL COMPLETE (ÖNCEKİ KAYDI SİLEREK)
app.MapPost("/api/dazmal/{id}/complete", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();

        // Dazmal bilgilerini al
        var dazmal = await db.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT siparis_no, urun_adi, baslama_tarihi FROM dazmal WHERE dazmal_id = @id",
            new { id });

        if (dazmal == null)
            return Results.NotFound(new { success = false, message = "Dazmal kaydı bulunamadı!" });

        var bitisTarihi = DateTime.Now;

        // 1. ÖNCE kesme bölümüne yeni kayıt oluştur
        var kesmeId = "KES" + DateTime.Now.Ticks;
        await db.ExecuteAsync(@"
            INSERT INTO kesme (kesme_id, siparis_no, urun_adi, baslama_tarihi, aciklama)
            VALUES (@kesmeId, @siparis_no, @urun_adi, @baslamaTarihi, @aciklama)",
            new
            {
                kesmeId,
                dazmal.siparis_no,
                dazmal.urun_adi,
                baslamaTarihi = bitisTarihi,
                aciklama = $"Dazmaldan otomatik geçiş - {id}"
            });

        // 2. SONRA dazmal kaydını sil
        var affectedRows = await db.ExecuteAsync("DELETE FROM dazmal WHERE dazmal_id = @id", new { id });

        if (affectedRows > 0)
        {
            return Results.Ok(new
            {
                success = true,
                message = "Dazmal tamamlandı ve kesme bölümüne geçildi!",
                yeni_kesme_id = kesmeId
            });
        }
        else
        {
            return Results.NotFound(new { success = false, message = "Dazmal kaydı bulunamadı!" });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Complete hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ DASHBOARD HTML SAYFASI
app.MapGet("/dashboard", async (HttpContext context) =>
{
    var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "dashboard.html");
    if (File.Exists(htmlPath))
    {
        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(htmlContent);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("HTML dosyası bulunamadı");
    }
});

// ✅ MAKINESAAT SAYFASI
app.MapGet("/makinesaat", async (HttpContext context) =>
{
    var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "makinesaat.html");
    if (File.Exists(htmlPath))
    {
        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(htmlContent);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("makinesaat.html bulunamadı");
    }
});

// ✅ ANA SAYFAYI DASHBOARD'A YÖNLENDİR
app.MapGet("/", async (HttpContext context) =>
{
    context.Response.Redirect("/dashboard");
});

// ✅ ÜRÜN AKIŞ TAKİBİ API
app.MapGet("/api/urun-akis", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();

        var urunAkis = await db.QueryAsync<dynamic>(@"
            -- MakineSaat'te aktif ürünler
            SELECT siparis_no, urun_adi, siparis_miktar, 'MakineSaat' as mevcut_bolum, 
                   baslama_tarihi, gecen_sure, tamamlandi
            FROM makinesaat 
            WHERE tamamlandi = false
            
            UNION ALL
            
            -- Ekleme'de aktif ürünler  
            SELECT siparis_no, urun_adi, 0 as siparis_miktar, 'Ekleme' as mevcut_bolum,
                   baslama_tarihi, gecen_sure, tamamlandi
            FROM ekleme
            WHERE tamamlandi = false
            
            UNION ALL
            
            -- Dazmal'da aktif ürünler
            SELECT siparis_no, urun_adi, 0 as siparis_miktar, 'Dazmal' as mevcut_bolum,
                   baslama_tarihi, gecen_sure, tamamlandi
            FROM dazmal
            WHERE tamamlandi = false
            
            UNION ALL
            
            -- Kesme'de aktif ürünler
            SELECT siparis_no, urun_adi, 0 as siparis_miktar, 'Kesme' as mevcut_bolum,
                   baslama_tarihi, gecen_sure, tamamlandi
            FROM kesme
            WHERE tamamlandi = false
            
            UNION ALL
            
            -- Paketleme'de aktif ürünler
            SELECT siparis_no, urun_adi, 0 as siparis_miktar, 'Paketleme' as mevcut_bolum,
                   baslama_tarihi, gecen_sure, tamamlandi
            FROM paketleme
            WHERE tamamlandi = false
            
            UNION ALL
            
            -- HazırDokuma'da aktif ürünler
            SELECT siparis_no, urun_adi, 0 as siparis_miktar, 'HazırDokuma' as mevcut_bolum,
                   baslama_tarihi, gecen_sure, tamamlandi
            FROM hazirdokuma
            WHERE tamamlandi = false
            
            UNION ALL
            
            -- HazırMatbaa'da aktif ürünler
            SELECT siparis_no, urun_adi, 0 as siparis_miktar, 'HazırMatbaa' as mevcut_bolum,
                   kayit_tarihi as baslama_tarihi, 0 as gecen_sure, tamamlandi
            FROM hazirmatbaa
            WHERE tamamlandi = false
            
            ORDER BY baslama_tarihi DESC
        ");

        return Results.Ok(urunAkis);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ürün akış hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ TOPLAM SİPARİŞ SAYISI (BUNU EKLE)
app.MapGet("/api/toplam-siparis", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();
        var toplam = await db.QueryFirstOrDefaultAsync<int>("SELECT COUNT(DISTINCT siparis_no) FROM makinesaat");
        return Results.Ok(new { toplamSiparis = toplam });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ AKTİF ÜRÜN SAYISI (BUNU EKLE)
app.MapGet("/api/aktif-urun", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();
        var aktif = await db.QueryFirstOrDefaultAsync<int>(@"
            SELECT COUNT(*) FROM (
                SELECT DISTINCT siparis_no FROM makinesaat WHERE tamamlandi = false
                UNION SELECT DISTINCT siparis_no FROM ekleme WHERE tamamlandi = false
                UNION SELECT DISTINCT siparis_no FROM dazmal WHERE tamamlandi = false
                UNION SELECT DISTINCT siparis_no FROM kesme WHERE tamamlandi = false
                UNION SELECT DISTINCT siparis_no FROM paketleme WHERE tamamlandi = false
            ) as aktif_urunler
        ");
        return Results.Ok(new { aktifUrun = aktif });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ EKLEME CRUD API'LERİ
app.MapGet("/api/ekleme", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();
        var eklemeList = await db.QueryAsync<dynamic>("SELECT * FROM ekleme ORDER BY kayit_tarihi DESC");
        return Results.Ok(eklemeList);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

app.MapPost("/api/ekleme", async (NpgsqlConnection db, Ekleme model) =>
{
    try
    {
        await db.OpenAsync();

        model.ekleme_id = "EKM" + DateTime.Now.Ticks;
        model.kayit_tarihi = DateTime.Now;

        await db.ExecuteAsync(@"
            INSERT INTO ekleme (ekleme_id, siparis_no, urun_adi, aciklama, baslama_tarihi, kayit_tarihi)
            VALUES (@ekleme_id, @siparis_no, @urun_adi, @aciklama, @baslama_tarihi, @kayit_tarihi)",
            model);

        return Results.Ok(new { success = true, message = "Ekleme kaydı eklendi!", id = model.ekleme_id });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ekleme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

app.MapDelete("/api/ekleme/{id}", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();
        var affectedRows = await db.ExecuteAsync("DELETE FROM ekleme WHERE ekleme_id = @id", new { id });

        if (affectedRows > 0)
            return Results.Ok(new { success = true, message = "Ekleme kaydı silindi!" });
        else
            return Results.NotFound(new { success = false, message = "Ekleme kaydı bulunamadı!" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Silme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ EKLEME COMPLETE (ÖNCEKİ KAYDI SİLEREK)
app.MapPost("/api/ekleme/{id}/complete", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();

        // Ekleme bilgilerini al
        var ekleme = await db.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT siparis_no, urun_adi, baslama_tarihi FROM ekleme WHERE ekleme_id = @id",
            new { id });

        if (ekleme == null)
            return Results.NotFound(new { success = false, message = "Ekleme kaydı bulunamadı!" });

        var bitisTarihi = DateTime.Now;

        // 1. ÖNCE dazmal bölümüne yeni kayıt oluştur
        var dazmalId = "DAZ" + DateTime.Now.Ticks;
        await db.ExecuteAsync(@"
            INSERT INTO dazmal (dazmal_id, siparis_no, urun_adi, baslama_tarihi, aciklama)
            VALUES (@dazmalId, @siparis_no, @urun_adi, @baslamaTarihi, @aciklama)",
            new
            {
                dazmalId,
                ekleme.siparis_no,
                ekleme.urun_adi,
                baslamaTarihi = bitisTarihi,
                aciklama = $"Eklemeden otomatik geçiş - {id}"
            });

        // 2. SONRA ekleme kaydını sil
        var affectedRows = await db.ExecuteAsync("DELETE FROM ekleme WHERE ekleme_id = @id", new { id });

        if (affectedRows > 0)
        {
            return Results.Ok(new
            {
                success = true,
                message = "Ekleme tamamlandı ve dazmal bölümüne geçildi!",
                yeni_dazmal_id = dazmalId
            });
        }
        else
        {
            return Results.NotFound(new { success = false, message = "Ekleme kaydı bulunamadı!" });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Complete hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ EKLEME SAYFASI
app.MapGet("/ekleme", async (HttpContext context) =>
{
    var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "ekleme.html");
    if (File.Exists(htmlPath))
    {
        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(htmlContent);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("ekleme.html bulunamadı");
    }
});

// ✅ DAZMAL CRUD API'LERİ
app.MapGet("/api/dazmal", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();
        var dazmalList = await db.QueryAsync<dynamic>("SELECT * FROM dazmal ORDER BY kayit_tarihi DESC");
        return Results.Ok(dazmalList);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

app.MapPost("/api/dazmal", async (NpgsqlConnection db, Dazmal model) =>
{
    try
    {
        await db.OpenAsync();

        model.dazmal_id = "DAZ" + DateTime.Now.Ticks;
        model.kayit_tarihi = DateTime.Now;

        await db.ExecuteAsync(@"
            INSERT INTO dazmal (dazmal_id, siparis_no, urun_adi, aciklama, baslama_tarihi, kayit_tarihi)
            VALUES (@dazmal_id, @siparis_no, @urun_adi, @aciklama, @baslama_tarihi, @kayit_tarihi)",
            model);

        return Results.Ok(new { success = true, message = "Dazmal kaydı eklendi!", id = model.dazmal_id });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Dazmal hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

app.MapDelete("/api/dazmal/{id}", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();
        var affectedRows = await db.ExecuteAsync("DELETE FROM dazmal WHERE dazmal_id = @id", new { id });

        if (affectedRows > 0)
            return Results.Ok(new { success = true, message = "Dazmal kaydı silindi!" });
        else
            return Results.NotFound(new { success = false, message = "Dazmal kaydı bulunamadı!" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Silme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});


// ✅ DAZMAL SAYFASI
app.MapGet("/dazmal", async (HttpContext context) =>
{
    var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "dazmal.html");
    if (File.Exists(htmlPath))
    {
        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(htmlContent);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("dazmal.html bulunamadı");
    }
});

// ✅ KESME CRUD API'LERİ
app.MapGet("/api/kesme", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();
        var kesmeList = await db.QueryAsync<dynamic>("SELECT * FROM kesme ORDER BY kayit_tarihi DESC");
        return Results.Ok(kesmeList);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

app.MapPost("/api/kesme", async (NpgsqlConnection db, Kesme model) =>
{
    try
    {
        await db.OpenAsync();

        model.kesme_id = "KES" + DateTime.Now.Ticks;
        model.kayit_tarihi = DateTime.Now;

        await db.ExecuteAsync(@"
            INSERT INTO kesme (kesme_id, siparis_no, urun_adi, aciklama, baslama_tarihi, kayit_tarihi)
            VALUES (@kesme_id, @siparis_no, @urun_adi, @aciklama, @baslama_tarihi, @kayit_tarihi)",
            model);

        return Results.Ok(new { success = true, message = "Kesme kaydı eklendi!", id = model.kesme_id });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Kesme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

app.MapDelete("/api/kesme/{id}", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();
        var affectedRows = await db.ExecuteAsync("DELETE FROM kesme WHERE kesme_id = @id", new { id });

        if (affectedRows > 0)
            return Results.Ok(new { success = true, message = "Kesme kaydı silindi!" });
        else
            return Results.NotFound(new { success = false, message = "Kesme kaydı bulunamadı!" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Silme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ KESME COMPLETE (ÖNCEKİ KAYDI SİLEREK)
app.MapPost("/api/kesme/{id}/complete", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();

        // Kesme bilgilerini al
        var kesme = await db.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT siparis_no, urun_adi, baslama_tarihi FROM kesme WHERE kesme_id = @id",
            new { id });

        if (kesme == null)
            return Results.NotFound(new { success = false, message = "Kesme kaydı bulunamadı!" });

        var bitisTarihi = DateTime.Now;

        // 1. ÖNCE paketleme bölümüne yeni kayıt oluştur
        var paketlemeId = "PAK" + DateTime.Now.Ticks;
        await db.ExecuteAsync(@"
            INSERT INTO paketleme (paketleme_id, siparis_no, urun_adi, baslama_tarihi, aciklama)
            VALUES (@paketlemeId, @siparis_no, @urun_adi, @baslamaTarihi, @aciklama)",
            new
            {
                paketlemeId,
                kesme.siparis_no,
                kesme.urun_adi,
                baslamaTarihi = bitisTarihi,
                aciklama = $"Kesmeden otomatik geçiş - {id}"
            });

        // 2. SONRA kesme kaydını sil
        var affectedRows = await db.ExecuteAsync("DELETE FROM kesme WHERE kesme_id = @id", new { id });

        if (affectedRows > 0)
        {
            return Results.Ok(new
            {
                success = true,
                message = "Kesme tamamlandı ve paketleme bölümüne geçildi!",
                yeni_paketleme_id = paketlemeId
            });
        }
        else
        {
            return Results.NotFound(new { success = false, message = "Kesme kaydı bulunamadı!" });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Complete hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ KESME SAYFASI
app.MapGet("/kesme", async (HttpContext context) =>
{
    var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "kesme.html");
    if (File.Exists(htmlPath))
    {
        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(htmlContent);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("kesme.html bulunamadı");
    }
});

// ✅ PAKETLEME CRUD API'LERİ
app.MapGet("/api/paketleme", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();
        var paketlemeList = await db.QueryAsync<dynamic>("SELECT * FROM paketleme ORDER BY kayit_tarihi DESC");
        return Results.Ok(paketlemeList);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

app.MapPost("/api/paketleme", async (NpgsqlConnection db, Paketleme model) =>
{
    try
    {
        await db.OpenAsync();

        model.paketleme_id = "PAK" + DateTime.Now.Ticks;
        model.kayit_tarihi = DateTime.Now;

        await db.ExecuteAsync(@"
            INSERT INTO paketleme (paketleme_id, siparis_no, urun_adi, aciklama, baslama_tarihi, kayit_tarihi)
            VALUES (@paketleme_id, @siparis_no, @urun_adi, @aciklama, @baslama_tarihi, @kayit_tarihi)",
            model);

        return Results.Ok(new { success = true, message = "Paketleme kaydı eklendi!", id = model.paketleme_id });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Paketleme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

app.MapDelete("/api/paketleme/{id}", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();
        var affectedRows = await db.ExecuteAsync("DELETE FROM paketleme WHERE paketleme_id = @id", new { id });

        if (affectedRows > 0)
            return Results.Ok(new { success = true, message = "Paketleme kaydı silindi!" });
        else
            return Results.NotFound(new { success = false, message = "Paketleme kaydı bulunamadı!" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Silme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ PAKETLEME COMPLETE (ÖNCEKİ KAYDI SİLEREK)
app.MapPost("/api/paketleme/{id}/complete", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();

        // Paketleme bilgilerini al
        var paketleme = await db.QueryFirstOrDefaultAsync<dynamic>(
            "SELECT siparis_no, urun_adi, baslama_tarihi FROM paketleme WHERE paketleme_id = @id",
            new { id });

        if (paketleme == null)
            return Results.NotFound(new { success = false, message = "Paketleme kaydı bulunamadı!" });

        var bitisTarihi = DateTime.Now;

        // 1. ÖNCE hazır dokuma bölümüne yeni kayıt oluştur
        var hazirDokumaId = "HDK" + DateTime.Now.Ticks;
        await db.ExecuteAsync(@"
            INSERT INTO hazirdokuma (hazirdokuma_id, siparis_no, urun_adi, baslama_tarihi, aciklama)
            VALUES (@hazirDokumaId, @siparis_no, @urun_adi, @baslamaTarihi, @aciklama)",
            new
            {
                hazirDokumaId,
                paketleme.siparis_no,
                paketleme.urun_adi,
                baslamaTarihi = bitisTarihi,
                aciklama = $"Paketlemeden otomatik geçiş - {id}"
            });

        // 2. SONRA paketleme kaydını sil
        var affectedRows = await db.ExecuteAsync("DELETE FROM paketleme WHERE paketleme_id = @id", new { id });

        if (affectedRows > 0)
        {
            return Results.Ok(new
            {
                success = true,
                message = "Paketleme tamamlandı ve hazır dokuma bölümüne geçildi!",
                yeni_hazirdokuma_id = hazirDokumaId
            });
        }
        else
        {
            return Results.NotFound(new { success = false, message = "Paketleme kaydı bulunamadı!" });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Complete hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});
// ✅ PAKETLEME SAYFASI
app.MapGet("/paketleme", async (HttpContext context) =>
{
    var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "paketleme.html");
    if (File.Exists(htmlPath))
    {
        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(htmlContent);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("paketleme.html bulunamadı");
    }
});


// ✅ PWA MANIFEST
app.MapGet("/manifest.json", async (HttpContext context) =>
{
    var manifest = @"
    {
        ""name"": ""DOSSOKAM Üretim Takip"",
        ""short_name"": ""DOSSOKAM"",
        ""start_url"": ""/"",
        ""display"": ""standalone"",
        ""background_color"": ""#ffffff"",
        ""theme_color"": ""#3498db"",
        ""icons"": [
            {
                ""src"": ""/icon-192.png"",
                ""sizes"": ""192x192"",
                ""type"": ""image/png""
            }
        ]
    }";

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync(manifest);
});

// ✅ PWA SERVICE WORKER (Basit)
app.MapGet("/sw.js", async (HttpContext context) =>
{
    var sw = @"
    self.addEventListener('install', event => {
        self.skipWaiting();
    });
    
    self.addEventListener('fetch', event => {
        event.respondWith(fetch(event.request));
    });
    ";

    context.Response.ContentType = "application/javascript";
    await context.Response.WriteAsync(sw);
});

// ✅ GÜNCELLENMİŞ ÜRÜN AKIŞ API - HAZIR DOKUMA VE MATBAA DAHİL
app.MapGet("/api/urun-akis-detay", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();

        var urunAkis = await db.QueryAsync<dynamic>(@"
            SELECT 
                siparis_no, urun_adi, siparis_miktar, mevcut_bolum,
                baslama_tarihi, gecen_sure, tamamlandi,
                -- TOPLAM SÜRE HESAPLA
                COALESCE((
                    SELECT SUM(gecen_sure) 
                    FROM (
                        SELECT gecen_sure FROM makinesaat WHERE siparis_no = m.siparis_no
                        UNION ALL SELECT gecen_sure FROM ekleme WHERE siparis_no = m.siparis_no
                        UNION ALL SELECT gecen_sure FROM dazmal WHERE siparis_no = m.siparis_no
                        UNION ALL SELECT gecen_sure FROM kesme WHERE siparis_no = m.siparis_no
                        UNION ALL SELECT gecen_sure FROM paketleme WHERE siparis_no = m.siparis_no
                        UNION ALL SELECT gecen_sure FROM hazirdokuma WHERE siparis_no = m.siparis_no
                        UNION ALL SELECT gecen_sure FROM hazirmatbaa WHERE siparis_no = m.siparis_no
                    ) AS tum_sureler
                ), 0) AS toplam_sure
                
            FROM (
                -- MakineSaat'te aktif ürünler
                SELECT siparis_no, urun_adi, siparis_miktar, 'MakineSaat' as mevcut_bolum, 
                       baslama_tarihi, gecen_sure, tamamlandi
                FROM makinesaat 
                WHERE tamamlandi = false
                
                UNION ALL
                -- Ekleme'de aktif ürünler  
                SELECT siparis_no, urun_adi, 0 as siparis_miktar, 'Ekleme' as mevcut_bolum,
                       baslama_tarihi, gecen_sure, tamamlandi
                FROM ekleme
                WHERE tamamlandi = false
                
                UNION ALL
                -- Dazmal'da aktif ürünler
                SELECT siparis_no, urun_adi, 0 as siparis_miktar, 'Dazmal' as mevcut_bolum,
                       baslama_tarihi, gecen_sure, tamamlandi
                FROM dazmal
                WHERE tamamlandi = false
                
                UNION ALL
                -- Kesme'de aktif ürünler
                SELECT siparis_no, urun_adi, 0 as siparis_miktar, 'Kesme' as mevcut_bolum,
                       baslama_tarihi, gecen_sure, tamamlandi
                FROM kesme
                WHERE tamamlandi = false
                
                UNION ALL
                -- Paketleme'de aktif ürünler
                SELECT siparis_no, urun_adi, 0 as siparis_miktar, 'Paketleme' as mevcut_bolum,
                       baslama_tarihi, gecen_sure, tamamlandi
                FROM paketleme
                WHERE tamamlandi = false
                
                UNION ALL
                -- HazırDokuma'da aktif ürünler
                SELECT siparis_no, urun_adi, 0 as siparis_miktar, 'HazırDokuma' as mevcut_bolum,
                       baslama_tarihi, gecen_sure, tamamlandi
                FROM hazirdokuma
                WHERE tamamlandi = false
                
                UNION ALL
                -- HazırMatbaa'da aktif ürünler
                SELECT siparis_no, urun_adi, 0 as siparis_miktar, 'HazırMatbaa' as mevcut_bolum,
                       kayit_tarihi as baslama_tarihi, 0 as gecen_sure, tamamlandi
                FROM hazirmatbaa
                WHERE tamamlandi = false
                
            ) AS m
            ORDER BY baslama_tarihi DESC
        ");

        return Results.Ok(urunAkis);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ürün akış hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ HAZIR DOKUMA CRUD API'LERİ
app.MapGet("/api/hazirdokuma", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();
        var hazirDokumaList = await db.QueryAsync<dynamic>("SELECT * FROM hazirdokuma ORDER BY kayit_tarihi DESC");
        return Results.Ok(hazirDokumaList);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

app.MapPost("/api/hazirdokuma", async (NpgsqlConnection db, HazirDokuma model) =>
{
    try
    {
        await db.OpenAsync();

        model.hazirdokuma_id = "HDK" + DateTime.Now.Ticks;
        model.kayit_tarihi = DateTime.Now;

        await db.ExecuteAsync(@"
            INSERT INTO hazirdokuma (hazirdokuma_id, siparis_no, urun_adi, aciklama, baslama_tarihi, kayit_tarihi)
            VALUES (@hazirdokuma_id, @siparis_no, @urun_adi, @aciklama, @baslama_tarihi, @kayit_tarihi)",
            model);

        return Results.Ok(new { success = true, message = "HazırDokuma kaydı eklendi!", id = model.hazirdokuma_id });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ekleme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

app.MapDelete("/api/hazirdokuma/{id}", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();
        var affectedRows = await db.ExecuteAsync("DELETE FROM hazirdokuma WHERE hazirdokuma_id = @id", new { id });

        if (affectedRows > 0)
            return Results.Ok(new { success = true, message = "HazırDokuma kaydı silindi!" });
        else
            return Results.NotFound(new { success = false, message = "HazırDokuma kaydı bulunamadı!" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Silme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ PAKETLEME'DEN HAZIR DOKUMA'YA OTOMATİK GEÇİŞ
app.MapPost("/api/paketleme/{id}/complete-to-hazirdokuma", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();

        // Paketleme kaydını tamamla
        var bitisTarihi = DateTime.Now;
        var affectedRows = await db.ExecuteAsync(@"
            UPDATE paketleme 
            SET tamamlandi = true, bitis_tarihi = @bitisTarihi, 
                gecen_sure = EXTRACT(EPOCH FROM (@bitisTarihi - baslama_tarihi))/3600
            WHERE paketleme_id = @id",
            new { id, bitisTarihi });

        if (affectedRows > 0)
        {
            // Paketleme bilgilerini al
            var paketleme = await db.QueryFirstOrDefaultAsync<dynamic>(
                "SELECT siparis_no, urun_adi, bitis_tarihi FROM paketleme WHERE paketleme_id = @id",
                new { id });

            if (paketleme != null)
            {
                // HazırDokuma bölümüne otomatik başlat
                var hazirDokumaId = "HDK" + DateTime.Now.Ticks;
                await db.ExecuteAsync(@"
                    INSERT INTO hazirdokuma (hazirdokuma_id, siparis_no, urun_adi, baslama_tarihi, aciklama)
                    VALUES (@hazirDokumaId, @siparis_no, @urun_adi, @baslamaTarihi, @aciklama)",
                    new
                    {
                        hazirDokumaId,
                        paketleme.siparis_no,
                        paketleme.urun_adi,
                        baslamaTarihi = paketleme.bitis_tarihi ?? bitisTarihi,
                        aciklama = $"Paketlemeden otomatik geçiş - {id}"
                    });

                return Results.Ok(new
                {
                    success = true,
                    message = "Paketleme tamamlandı ve HazırDokuma'ya geçildi!",
                    yeni_hazirdokuma_id = hazirDokumaId
                });
            }
        }

        return Results.NotFound(new { success = false, message = "Paketleme kaydı bulunamadı!" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Complete hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ HAZIR DOKUMA SAYFASI
app.MapGet("/hazirdokuma", async (HttpContext context) =>
{
    var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "hazirdokuma.html");
    if (File.Exists(htmlPath))
    {
        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(htmlContent);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("hazirdokuma.html bulunamadı");
    }
});

// ✅ HAZIR DOKUMA COMPLETE API
app.MapPost("/api/hazirdokuma/{id}/complete", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();

        var bitisTarihi = DateTime.Now;
        var affectedRows = await db.ExecuteAsync(@"
            UPDATE hazirdokuma 
            SET tamamlandi = true, bitis_tarihi = @bitisTarihi, 
                gecen_sure = EXTRACT(EPOCH FROM (@bitisTarihi - baslama_tarihi))/3600
            WHERE hazirdokuma_id = @id",
            new { id, bitisTarihi });

        if (affectedRows > 0)
        {
            return Results.Ok(new
            {
                success = true,
                message = "Hazır Dokuma tamamlandı! Ürün hazır."
            });
        }
        else
        {
            return Results.NotFound(new { success = false, message = "Hazır Dokuma kaydı bulunamadı!" });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Complete hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ BASİTLEŞTİRİLMİŞ EXCEL SAYFASI
app.MapGet("/excel", async (HttpContext context) =>
{
    var html = @"
    <!DOCTYPE html>
    <html lang='tr'>
    <head>
        <meta charset='UTF-8'>
        <meta name='viewport' content='width=device-width, initial-scale=1.0'>
        <title>Excel Paylaşım - DOSSOKAM</title>
        <style>
            /* CSS STİLLERİ AYNI KALACAK */
            * { margin: 0; padding: 0; box-sizing: border-box; }
            body { font-family: Arial, sans-serif; background: #f5f5f5; padding: 10px; font-size: 14px; }
            .container { max-width: 100%; margin: 0 auto; }
            .header { background: #2c3e50; color: white; padding: 15px; border-radius: 10px; margin-bottom: 15px; text-align: center; }
            .header h1 { font-size: 1.4em; margin-bottom: 5px; }
            .header p { font-size: 0.9em; opacity: 0.8; }
            .nav { display: grid; grid-template-columns: repeat(3, 1fr); gap: 8px; margin-bottom: 15px; }
            .nav-btn { background: #3498db; color: white; border: none; padding: 12px 8px; border-radius: 8px; text-decoration: none; font-size: 12px; text-align: center; display: flex; flex-direction: column; align-items: center; justify-content: center; min-height: 60px; }
            .nav-btn:hover { background: #2980b9; }
            .table-container { background: white; padding: 15px; border-radius: 10px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); margin-bottom: 15px; overflow-x: auto; }
            .btn { background: #27ae60; color: white; border: none; padding: 20px; border-radius: 8px; font-size: 16px; cursor: pointer; text-align: center; display: block; width: 100%; margin-bottom: 10px; font-weight: bold; }
            .btn:hover { background: #219652; }
            @media (max-width: 480px) {
                body { padding: 8px; }
                .nav { grid-template-columns: repeat(2, 1fr); }
                .nav-btn { padding: 10px 6px; font-size: 11px; min-height: 50px; }
                .header { padding: 12px; }
                .header h1 { font-size: 1.2em; }
            }
            @media (min-width: 768px) {
                .nav { grid-template-columns: repeat(6, 1fr); }
            }
        </style>
    </head>
    <body>
        <div class='container'>
            <div class='header'>
                <h1>📗 Excel Paylaşım</h1>
                <p>Tüm üretim verilerini tek dosyada indirin</p>
            </div>
            
            <div class='nav'>
                <a href='/dashboard' class='nav-btn'>📊<br>Dashboard</a>
                <a href='/makinesaat' class='nav-btn'>🔴<br>Makine</a>
                <a href='/ekleme' class='nav-btn'>🟠<br>Ekleme</a>
                <a href='/dazmal' class='nav-btn'>🟡<br>Dazmal</a>
                <a href='/kesme' class='nav-btn'>🔵<br>Kesme</a>
                <a href='/paketleme' class='nav-btn'>🟣<br>Paketleme</a>
                <a href='/hazirdokuma' class='nav-btn'>🟢<br>HazırDokuma</a>
                <a href='/hazirmatbaa' class='nav-btn'>🔶<br>HazırMatbaa</a>
                <a href='/excel' class='nav-btn'>📗<br>ExcelPaylas</a>
            </div>

            <div class='table-container'>
                <h2 style='margin-bottom: 20px; text-align: center;'>📊 Tüm Üretim Verilerini İndir</h2>
                <button class='btn' onclick='exportToExcel()'>
                    📥 EXCEL VERİLERİ AKTAR<br>
                    <small>Tüm bölümler tek dosyada - 3 sayfa halinde</small>
                </button>
            </div>

            <div class='table-container'>
                <h3>📋 İçindekiler</h3>
                <ul style='padding-left: 20px;'>
                    <li><strong>Sayfa 1:</strong> Üretim Akışı (MakineSaat'ten HazırDokuma'ya)</li>
                    <li><strong>Sayfa 2:</strong> Hazır Matbaa İşlemleri</li>
                    <li><strong>Sayfa 3:</strong> Üretim Özeti ve İstatistikler</li>
                </ul>
                <p style='margin-top: 10px; color: #666;'>
                    💡 <strong>Not:</strong> CSV dosyasını Excel'de açtıktan sonra 'Veri > Metni Sütunlara Dönüştür' seçeneği ile sütunlara ayırabilirsiniz.
                </p>
            </div>
        </div>

        <script>
            async function exportToExcel() {
                try {
                    const response = await fetch('/api/excel-export-real');
                    
                    if (response.ok) {
                        const blob = await response.blob();
                        const url = window.URL.createObjectURL(blob);
                        const a = document.createElement('a');
                        a.href = url;
                        a.download = 'DOSSOGROUP_URETIM_' + new Date().toISOString().split('T')[0] + '.csv';
                        document.body.appendChild(a);
                        a.click();
                        window.URL.revokeObjectURL(url);
                        document.body.removeChild(a);
                        
                        setTimeout(() => {
                            alert('✅ Tüm üretim verileri başarıyla indirildi!\\n\\nDosya 3 bölümden oluşmaktadır:\\n• Üretim Akışı\\n• Hazır Matbaa\\n• Üretim Özeti');
                        }, 1000);
                        
                    } else {
                        alert('❌ Excel oluşturma hatası!');
                    }
                    
                } catch (error) {
                    alert('❌ Hata: ' + error.message);
                }
            }
        </script>
    </body>
    </html>";

    context.Response.ContentType = "text/html";
    await context.Response.WriteAsync(html);
});


// ✅ HAZIR MATBAA CRUD API'LERİ
app.MapGet("/api/hazirmatbaa", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();
        var hazirMatbaaList = await db.QueryAsync<dynamic>("SELECT * FROM hazirmatbaa ORDER BY kayit_tarihi DESC");
        return Results.Ok(hazirMatbaaList);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Hata: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

app.MapPost("/api/hazirmatbaa", async (NpgsqlConnection db, HazirMatbaa model) =>
{
    try
    {
        await db.OpenAsync();

        model.hazirmatbaa_id = "HMB" + DateTime.Now.Ticks;
        model.kayit_tarihi = DateTime.Now;

        await db.ExecuteAsync(@"
            INSERT INTO hazirmatbaa (hazirmatbaa_id, siparis_no, urun_adi, aciklama, kayit_tarihi, kaydeden_kullanici_id)
            VALUES (@hazirmatbaa_id, @siparis_no, @urun_adi, @aciklama, @kayit_tarihi, @kaydeden_kullanici_id)",
            model);

        return Results.Ok(new { success = true, message = "Hazır Matbaa kaydı eklendi!", id = model.hazirmatbaa_id });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Ekleme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

app.MapDelete("/api/hazirmatbaa/{id}", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();
        var affectedRows = await db.ExecuteAsync("DELETE FROM hazirmatbaa WHERE hazirmatbaa_id = @id", new { id });

        if (affectedRows > 0)
            return Results.Ok(new { success = true, message = "Hazır Matbaa kaydı silindi!" });
        else
            return Results.NotFound(new { success = false, message = "Hazır Matbaa kaydı bulunamadı!" });
    }
    catch (Exception ex)
    {
        return Results.Problem($"Silme hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ HAZIR MATBAA COMPLETE API
app.MapPost("/api/hazirmatbaa/{id}/complete", async (NpgsqlConnection db, string id) =>
{
    try
    {
        await db.OpenAsync();

        // HazırMatbaa'da tamamlandı işareti yap (kayıt kalacak, sadece durum değişecek)
        var affectedRows = await db.ExecuteAsync(@"
            UPDATE hazirmatbaa 
            SET tamamlandi = true
            WHERE hazirmatbaa_id = @id",
            new { id });

        if (affectedRows > 0)
        {
            return Results.Ok(new
            {
                success = true,
                message = "✅ Hazır Matbaa tamamlandı! Ürün matbaa işlemleri bitmiştir."
            });
        }
        else
        {
            return Results.NotFound(new { success = false, message = "Hazır Matbaa kaydı bulunamadı!" });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Complete hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ HAZIR MATBAA SAYFASI
app.MapGet("/hazirmatbaa", async (HttpContext context) =>
{
    var htmlPath = Path.Combine(Directory.GetCurrentDirectory(), "hazirmatbaa.html");
    if (File.Exists(htmlPath))
    {
        var htmlContent = await File.ReadAllTextAsync(htmlPath);
        context.Response.ContentType = "text/html";
        await context.Response.WriteAsync(htmlContent);
    }
    else
    {
        context.Response.StatusCode = 404;
        await context.Response.WriteAsync("hazirmatbaa.html bulunamadı");
    }
});

// ✅ DASHBOARD TÜM BÖLÜMLER API
app.MapGet("/api/dashboard-tum-bolumler", async (NpgsqlConnection db) =>
{
    try
    {
        await db.OpenAsync();

        var result = new
        {
            makinesaat = await db.QueryAsync<dynamic>("SELECT * FROM makinesaat WHERE tamamlandi = false ORDER BY kayit_tarihi DESC"),
            ekleme = await db.QueryAsync<dynamic>("SELECT * FROM ekleme WHERE tamamlandi = false ORDER BY kayit_tarihi DESC"),
            kesme = await db.QueryAsync<dynamic>("SELECT * FROM kesme WHERE tamamlandi = false ORDER BY kayit_tarihi DESC"),
            paketleme = await db.QueryAsync<dynamic>("SELECT * FROM paketleme WHERE tamamlandi = false ORDER BY kayit_tarihi DESC"),
            hazirdokuma = await db.QueryAsync<dynamic>("SELECT * FROM hazirdokuma WHERE tamamlandi = false ORDER BY kayit_tarihi DESC"),
            hazirmatbaa = await db.QueryAsync<dynamic>("SELECT * FROM hazirmatbaa WHERE tamamlandi = false ORDER BY kayit_tarihi DESC")
        };

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem($"Dashboard bölümler hatası: {ex.Message}");
    }
    finally
    {
        await db.CloseAsync();
    }
});

// ✅ HELPER FONKSİYONLAR
async Task<IEnumerable<dynamic>> GetMakineSaatRecords()
{
    using var db = new NpgsqlConnection("Host=dpg-d40ugp75r7bs7388bsrg-a.oregon-postgres.render.com;Port=5432;Database=dokuma_takip;Username=dokuma_user;Password=Pm41PLYnjRqukLgaUL2vetPrkZxiBOrq;SSL Mode=Require;Trust Server Certificate=true;");
    await db.OpenAsync();
    return await db.QueryAsync<dynamic>("SELECT * FROM makinesaat WHERE tamamlandi = false ORDER BY kayit_tarihi DESC");
}

async Task<IEnumerable<dynamic>> GetEklemeRecords()
{
    using var db = new NpgsqlConnection("Host=dpg-d40ugp75r7bs7388bsrg-a.oregon-postgres.render.com;Port=5432;Database=dokuma_takip;Username=dokuma_user;Password=Pm41PLYnjRqukLgaUL2vetPrkZxiBOrq;SSL Mode=Require;Trust Server Certificate=true;");
    await db.OpenAsync();
    return await db.QueryAsync<dynamic>("SELECT * FROM ekleme WHERE tamamlandi = false ORDER BY kayit_tarihi DESC");
}

async Task<IEnumerable<dynamic>> GetKesmeRecords()
{
    using var db = new NpgsqlConnection("Host=dpg-d40ugp75r7bs7388bsrg-a.oregon-postgres.render.com;Port=5432;Database=dokuma_takip;Username=dokuma_user;Password=Pm41PLYnjRqukLgaUL2vetPrkZxiBOrq;SSL Mode=Require;Trust Server Certificate=true;");
    await db.OpenAsync();
    return await db.QueryAsync<dynamic>("SELECT * FROM kesme WHERE tamamlandi = false ORDER BY kayit_tarihi DESC");
}

async Task<IEnumerable<dynamic>> GetPaketlemeRecords()
{
    using var db = new NpgsqlConnection("Host=dpg-d40ugp75r7bs7388bsrg-a.oregon-postgres.render.com;Port=5432;Database=dokuma_takip;Username=dokuma_user;Password=Pm41PLYnjRqukLgaUL2vetPrkZxiBOrq;SSL Mode=Require;Trust Server Certificate=true;");
    await db.OpenAsync();
    return await db.QueryAsync<dynamic>("SELECT * FROM paketleme WHERE tamamlandi = false ORDER BY kayit_tarihi DESC");
}

async Task<IEnumerable<dynamic>> GetHazirDokumaRecords()
{
    using var db = new NpgsqlConnection("Host=dpg-d40ugp75r7bs7388bsrg-a.oregon-postgres.render.com;Port=5432;Database=dokuma_takip;Username=dokuma_user;Password=Pm41PLYnjRqukLgaUL2vetPrkZxiBOrq;SSL Mode=Require;Trust Server Certificate=true;");
    await db.OpenAsync();
    return await db.QueryAsync<dynamic>("SELECT * FROM hazirdokuma WHERE tamamlandi = false ORDER BY kayit_tarihi DESC");
}

async Task<IEnumerable<dynamic>> GetHazirMatbaaRecords()
{
    using var db = new NpgsqlConnection("Host=dpg-d40ugp75r7bs7388bsrg-a.oregon-postgres.render.com;Port=5432;Database=dokuma_takip;Username=dokuma_user;Password=Pm41PLYnjRqukLgaUL2vetPrkZxiBOrq;SSL Mode=Require;Trust Server Certificate=true;");
    await db.OpenAsync();
    return await db.QueryAsync<dynamic>("SELECT * FROM hazirmatbaa WHERE tamamlandi = false ORDER BY kayit_tarihi DESC");
}

// ✅ EXCEL EXPORT API - 2 SAYFALI
app.MapGet("/api/excel-export-real", async (NpgsqlConnection db) =>
{
    try
    {
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        using var package = new OfficeOpenXml.ExcelPackage();

        // SAYFA 1: ÜRETİM AKIŞI
        var worksheet1 = package.Workbook.Worksheets.Add("Üretim Akışı");
        worksheet1.Cells[1, 1].Value = "Sipariş No";
        worksheet1.Cells[1, 2].Value = "Ürün Adı";
        worksheet1.Cells[1, 3].Value = "Bölüm";
        worksheet1.Cells[1, 4].Value = "Durum";
        worksheet1.Cells[1, 5].Value = "Başlama Tarihi";
        worksheet1.Cells[1, 6].Value = "Kayıt Tarihi";

        // Stil
        using (var range = worksheet1.Cells[1, 1, 1, 6])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
        }

        int row = 2;
        await db.OpenAsync();

        // Üretim akışı verileri
        row = await WriteTableToExcel(worksheet1, row, db, "makinesaat", "MakineSaat");
        row = await WriteTableToExcel(worksheet1, row, db, "ekleme", "Ekleme");
        row = await WriteTableToExcel(worksheet1, row, db, "dazmal", "Dazmal");
        row = await WriteTableToExcel(worksheet1, row, db, "kesme", "Kesme");
        row = await WriteTableToExcel(worksheet1, row, db, "paketleme", "Paketleme");
        row = await WriteTableToExcel(worksheet1, row, db, "hazirdokuma", "HazırDokuma");

        worksheet1.Cells[worksheet1.Dimension.Address].AutoFitColumns();

        // SAYFA 2: HAZIR MATBAA
        var worksheet2 = package.Workbook.Worksheets.Add("Hazır Matbaa");
        worksheet2.Cells[1, 1].Value = "Sipariş No";
        worksheet2.Cells[1, 2].Value = "Ürün Adı";
        worksheet2.Cells[1, 3].Value = "Durum";
        worksheet2.Cells[1, 4].Value = "Kayıt Tarihi";
        worksheet2.Cells[1, 5].Value = "Açıklama";

        // Stil
        using (var range = worksheet2.Cells[1, 1, 1, 5])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);
        }

        int row2 = 2;
        var hazirmatbaaData = await db.QueryAsync<dynamic>("SELECT * FROM hazirmatbaa ORDER BY siparis_no");
        foreach (var item in hazirmatbaaData)
        {
            worksheet2.Cells[row2, 1].Value = item.siparis_no?.ToString();
            worksheet2.Cells[row2, 2].Value = item.urun_adi?.ToString();
            worksheet2.Cells[row2, 3].Value = (item.tamamlandi != null && (bool)item.tamamlandi) ? "Tamamlandı" : "Devam Ediyor";
            worksheet2.Cells[row2, 4].Value = item.kayit_tarihi != null ? ((DateTime)item.kayit_tarihi).ToString("yyyy-MM-dd HH:mm") : "";
            worksheet2.Cells[row2, 5].Value = item.aciklama?.ToString();
            row2++;
        }

        worksheet2.Cells[worksheet2.Dimension.Address].AutoFitColumns();

        var fileBytes = package.GetAsByteArray();
        return Results.File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"DOSSOKAM_URETIM_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        );

    }
    catch (Exception ex)
    {
        return Results.Problem($"Excel oluşturma hatası: {ex.Message}");
    }
});

// Yardımcı metod
async Task<int> WriteTableToExcel(OfficeOpenXml.ExcelWorksheet worksheet, int row, NpgsqlConnection db, string tableName, string bolumAdi)
{
    var data = await db.QueryAsync<dynamic>($"SELECT * FROM {tableName} ORDER BY siparis_no");

    foreach (var item in data)
    {
        worksheet.Cells[row, 1].Value = item.siparis_no?.ToString();
        worksheet.Cells[row, 2].Value = item.urun_adi?.ToString();
        worksheet.Cells[row, 3].Value = bolumAdi;
        worksheet.Cells[row, 4].Value = (item.tamamlandi != null && (bool)item.tamamlandi) ? "Tamamlandı" : "Devam Ediyor";
        worksheet.Cells[row, 5].Value = item.baslama_tarihi != null ? ((DateTime)item.baslama_tarihi).ToString("yyyy-MM-dd HH:mm") : "";
        worksheet.Cells[row, 6].Value = item.kayit_tarihi != null ? ((DateTime)item.kayit_tarihi).ToString("yyyy-MM-dd HH:mm") : "";
        row++;
    }

    return row;
}

//// ✅ EXCEL EXPORT API - EPPLUS 8+ UYUMLU
//app.MapGet("/api/excel-export-real", async (NpgsqlConnection db) =>
//{
//    try
//    {
//        // EPPlus 8+ lisans ayarı - SADECE BURADA
//        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

//        using var package = new OfficeOpenXml.ExcelPackage();
//        var worksheet = package.Workbook.Worksheets.Add("Dashboard");

//        // Başlık satırı
//        worksheet.Cells[1, 1].Value = "Sipariş No";
//        worksheet.Cells[1, 2].Value = "Ürün Adı";
//        worksheet.Cells[1, 3].Value = "Bölüm";
//        worksheet.Cells[1, 4].Value = "Durum";
//        worksheet.Cells[1, 5].Value = "Başlama Tarihi";
//        worksheet.Cells[1, 6].Value = "Kayıt Tarihi";
//        // Başlık stili
//        using (var range = worksheet.Cells[1, 1, 1, 6])
//        {
//            range.Style.Font.Bold = true;
//            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
//            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
//        }

//        int row = 2;
//        await db.OpenAsync();

//        // Tüm bölümlerden verileri çek ve Excel'e yaz
//        row = await WriteTableToExcel(worksheet, row, db, "makinesaat", "MakineSaat");
//        row = await WriteTableToExcel(worksheet, row, db, "ekleme", "Ekleme");
//        row = await WriteTableToExcel(worksheet, row, db, "dazmal", "Dazmal");
//        row = await WriteTableToExcel(worksheet, row, db, "kesme", "Kesme");
//        row = await WriteTableToExcel(worksheet, row, db, "paketleme", "Paketleme");
//        row = await WriteTableToExcel(worksheet, row, db, "hazirdokuma", "HazırDokuma");
//        row = await WriteTableToExcel(worksheet, row, db, "hazirmatbaa", "HazırMatbaa");

//        // Kolon genişliklerini ayarla
//        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

//        // Excel'i byte array'e çevir ve döndür
//        var fileBytes = package.GetAsByteArray();
//        return Results.File(
//            fileBytes,
//            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
//            $"DOSSOKAM_URETIM_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
//        );

//    }
//    catch (Exception ex)
//    {
//        return Results.Problem($"Excel oluşturma hatası: {ex.Message}");
//    }
//});

//// Yardımcı metod - Her tablo için verileri Excel'e yazar
//async Task<int> WriteTableToExcel(ExcelWorksheet worksheet, int row, NpgsqlConnection db, string tableName, string bolumAdi)
//{
//    var data = await db.QueryAsync<dynamic>($"SELECT * FROM {tableName} ORDER BY siparis_no");

//    foreach (var item in data)
//    {
//        worksheet.Cells[row, 1].Value = item.siparis_no?.ToString();
//        worksheet.Cells[row, 2].Value = item.urun_adi?.ToString();
//        worksheet.Cells[row, 3].Value = bolumAdi;
//        worksheet.Cells[row, 4].Value = (item.tamamlandi != null && (bool)item.tamamlandi) ? "Tamamlandı" : "Devam Ediyor";
//        worksheet.Cells[row, 5].Value = item.baslama_tarihi != null ? ((DateTime)item.baslama_tarihi).ToString("yyyy-MM-dd HH:mm") : "";
//        worksheet.Cells[row, 6].Value = item.kayit_tarihi != null ? ((DateTime)item.kayit_tarihi).ToString("yyyy-MM-dd HH:mm") : "";
//        row++;
//    }

//    return row;
//}

app.Run();

// 📋 MODELLER
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class MakineSaat
{
    public string? makinesaat_id { get; set; }
    public string makine_no { get; set; } = string.Empty;
    public string siparis_no { get; set; } = string.Empty;
    public string urun_adi { get; set; } = string.Empty;
    public int kalan_saat { get; set; }
    public int siparis_miktar { get; set; }
    public string? aciklama { get; set; }
    public string? dokumaci1_id { get; set; }
    public string? dokumaci2_id { get; set; }
    public DateTime? baslama_tarihi { get; set; }
    public DateTime? bitis_tarihi { get; set; }
    public int gecen_sure { get; set; }
    public bool tamamlandi { get; set; }
    public DateTime kayit_tarihi { get; set; }
}

// ✅ EKLEME MODELİ
public class Ekleme
{
    public string? ekleme_id { get; set; }
    public string siparis_no { get; set; } = string.Empty;
    public string urun_adi { get; set; } = string.Empty;
    public string? aciklama { get; set; }
    public DateTime? baslama_tarihi { get; set; }
    public DateTime? bitis_tarihi { get; set; }
    public int gecen_sure { get; set; }
    public bool tamamlandi { get; set; }
    public DateTime kayit_tarihi { get; set; }
}

// ✅ DAZMAL MODELİ
public class Dazmal
{
    public string? dazmal_id { get; set; }
    public string siparis_no { get; set; } = string.Empty;
    public string urun_adi { get; set; } = string.Empty;
    public string? aciklama { get; set; }
    public DateTime? baslama_tarihi { get; set; }
    public DateTime? bitis_tarihi { get; set; }
    public int gecen_sure { get; set; }
    public bool tamamlandi { get; set; }
    public DateTime kayit_tarihi { get; set; }
}

// ✅ KESME MODELİ
public class Kesme
{
    public string? kesme_id { get; set; }
    public string siparis_no { get; set; } = string.Empty;
    public string urun_adi { get; set; } = string.Empty;
    public string? aciklama { get; set; }
    public DateTime? baslama_tarihi { get; set; }
    public DateTime? bitis_tarihi { get; set; }
    public int gecen_sure { get; set; }
    public bool tamamlandi { get; set; }
    public DateTime kayit_tarihi { get; set; }
}

// ✅ PAKETLEME MODELİ
public class Paketleme
{
    public string? paketleme_id { get; set; }
    public string siparis_no { get; set; } = string.Empty;
    public string urun_adi { get; set; } = string.Empty;
    public string? aciklama { get; set; }
    public DateTime? baslama_tarihi { get; set; }
    public DateTime? bitis_tarihi { get; set; }
    public int gecen_sure { get; set; }
    public bool tamamlandi { get; set; }
    public DateTime kayit_tarihi { get; set; }
}

// ✅ HAZIR DOKUMA MODELİ
public class HazirDokuma
{
    public string? hazirdokuma_id { get; set; }
    public string siparis_no { get; set; } = string.Empty;
    public string urun_adi { get; set; } = string.Empty;
    public string? aciklama { get; set; }
    public DateTime? baslama_tarihi { get; set; }
    public DateTime? bitis_tarihi { get; set; }
    public int gecen_sure { get; set; }
    public bool tamamlandi { get; set; }
    public DateTime kayit_tarihi { get; set; }
}

// ✅ HAZIR MATBAA MODELİ
public class HazirMatbaa
{
    public string? hazirmatbaa_id { get; set; }
    public string siparis_no { get; set; } = string.Empty;
    public string urun_adi { get; set; } = string.Empty;
    public string? aciklama { get; set; }
    public DateTime kayit_tarihi { get; set; }
    public string? kaydeden_kullanici_id { get; set; }
    public bool tamamlandi { get; set; }

}
