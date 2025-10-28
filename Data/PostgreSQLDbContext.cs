using Microsoft.EntityFrameworkCore;
using DOSSOKAM2019.Models;

namespace DOSSOKAM2019.Data
{
    public class PostgreSQLDbContext : DbContext
    {
        public PostgreSQLDbContext(DbContextOptions<PostgreSQLDbContext> options) : base(options) { }

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
            // PostgreSQL için tablo isimleri küçük harf (opsiyonel)
            modelBuilder.Entity<MakineSaat>(entity =>
            {
                entity.ToTable("makinesaat");
                entity.HasKey(e => e.MakineSaatID);
                entity.Property(e => e.KalanSaat).HasColumnType("numeric(18,2)");
                entity.Property(e => e.GecenSure).HasColumnType("numeric(18,2)").IsRequired(false);
            });

            modelBuilder.Entity<Dokumaci>(entity =>
            {
                entity.ToTable("dokumacilar");
                entity.HasKey(e => e.DokumaciID);
            });

            // Diğer entity'ler için benzer configuration...
            
            base.OnModelCreating(modelBuilder);
        }
    }
}