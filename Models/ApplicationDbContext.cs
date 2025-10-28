using Microsoft.EntityFrameworkCore;

namespace DOSSOKAM2019.Models;  // ✅ BU SATIRI EKLE!

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Kullanici> Kullanicilar { get; set; }
    public DbSet<MakineSaat> MakineSaat { get; set; }
    public DbSet<Dokumaci> Dokumacilar { get; set; }
    public DbSet<Ekleme> Ekleme { get; set; }
    public DbSet<Dazmal> Dazmal { get; set; }
    public DbSet<Kesme> Kesme { get; set; }
    public DbSet<Paketleme> Paketleme { get; set; }
    public DbSet<HazirDokuma> HazirDokuma { get; set; }
    public DbSet<HazirMatbaa> HazirMatbaa { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MakineSaat>(entity =>
        {
            entity.ToTable("MakineSaat");
            entity.HasKey(e => e.MakineSaatID);
            entity.Property(e => e.KalanSaat).HasColumnType("decimal(18,2)");
            entity.Property(e => e.GecenSure)
                  .HasColumnType("decimal(18,2)")
                  .IsRequired(false)
                  .ValueGeneratedOnAddOrUpdate();
            entity.Property(e => e.Dokumaci1ID).IsRequired(false);
            entity.Property(e => e.Dokumaci2ID).IsRequired(false);
            entity.Property(e => e.BitisTarihi).IsRequired(false);
            entity.Property(e => e.Aciklama).IsRequired(false);
        });

        modelBuilder.Entity<Dokumaci>(entity =>
        {
            entity.ToTable("Dokumacilar");
            entity.HasKey(e => e.DokumaciID);
            entity.Property(e => e.Telefon).IsRequired(false);
            entity.Property(e => e.Adres).IsRequired(false);
        });

        modelBuilder.Entity<Kullanici>(entity =>
        {
            entity.ToTable("Kullanicilar");
            entity.HasKey(e => e.KullaniciID);
            entity.Property(e => e.Email).IsRequired(false);
            entity.Property(e => e.Role).IsRequired(false);
        });

        modelBuilder.Entity<Ekleme>(entity =>
        {
            entity.ToTable("Ekleme");
            entity.HasKey(e => e.EklemeID);
            entity.Property(e => e.BaslamaTarihi).IsRequired(false);
            entity.Property(e => e.BitisTarihi).IsRequired(false);
            entity.Property(e => e.Aciklama).IsRequired(false);
        });

        modelBuilder.Entity<Dazmal>(entity =>
        {
            entity.ToTable("Dazmal");
            entity.HasKey(e => e.DazmalID);
            entity.Property(e => e.BaslamaTarihi).IsRequired(false);
            entity.Property(e => e.BitisTarihi).IsRequired(false);
            entity.Property(e => e.Aciklama).IsRequired(false);
        });

        modelBuilder.Entity<Kesme>(entity =>
        {
            entity.ToTable("Kesme");
            entity.HasKey(e => e.KesmeID);
            entity.Property(e => e.GecenSure)
                  .HasColumnType("decimal(18,2)")
                  .IsRequired(false)
                  .ValueGeneratedOnAddOrUpdate();
            entity.Property(e => e.BaslamaTarihi).IsRequired(false);
            entity.Property(e => e.BitisTarihi).IsRequired(false);
            entity.Property(e => e.Aciklama).IsRequired(false);
        });

        modelBuilder.Entity<Paketleme>(entity =>
        {
            entity.ToTable("Paketleme");
            entity.HasKey(e => e.PaketlemeID);
            entity.Property(e => e.GecenSure)
                  .HasColumnType("decimal(18,2)")
                  .IsRequired(false)
                  .ValueGeneratedOnAddOrUpdate();
            entity.Property(e => e.BaslamaTarihi).IsRequired(false);
            entity.Property(e => e.BitisTarihi).IsRequired(false);
            entity.Property(e => e.Aciklama).IsRequired(false);
        });

        modelBuilder.Entity<HazirDokuma>(entity =>
        {
            entity.ToTable("HazirDokuma");
            entity.HasKey(e => e.HazirDokumaID);
            entity.Property(e => e.GecenSure)
                  .HasColumnType("decimal(18,2)")
                  .IsRequired(false)
                  .ValueGeneratedOnAddOrUpdate();
            entity.Property(e => e.CikanAdet).IsRequired(false);
            entity.Property(e => e.BaslamaTarihi).IsRequired(false);
            entity.Property(e => e.BitisTarihi).IsRequired(false);
            entity.Property(e => e.Aciklama).IsRequired(false);
        });

        modelBuilder.Entity<HazirMatbaa>(entity =>
        {
            entity.ToTable("HazirMatbaa");
            entity.HasKey(e => e.HazirMatbaaID);
            entity.Property(e => e.CikanAdet).IsRequired(false);
            entity.Property(e => e.Aciklama).IsRequired(false);
            entity.Property(e => e.KaydedenKullaniciID).IsRequired(false);
        });
    }

}
