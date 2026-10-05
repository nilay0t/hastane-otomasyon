using System.Text.Json;
using HastaneOtomasyon.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HastaneOtomasyon.Data.Context
{
    public class AppDbContext : DbContext
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false
        };

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public string CurrentKullaniciAdi { get; set; } = "Sistem";

        public DbSet<Rol> Roller { get; set; } = null!;

        public DbSet<Kullanici> Kullanicilar { get; set; } = null!;

        public DbSet<Poliklinik> Poliklinikler { get; set; } = null!;

        public DbSet<Hasta> Hastalar { get; set; } = null!;

        public DbSet<Doktor> Doktorlar { get; set; } = null!;

        public DbSet<Randevu> Randevular { get; set; } = null!;

        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        public override int SaveChanges()
        {
            return SaveChangesAsync().GetAwaiter().GetResult();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var auditLogs = CreateAuditLogs();
            if (auditLogs.Count > 0)
            {
                await AuditLogs.AddRangeAsync(auditLogs, cancellationToken);
            }

            return await base.SaveChangesAsync(cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            ConfigureSoftDeleteFilters(modelBuilder);
            ConfigureRelationships(modelBuilder);
        }

        private static void ConfigureSoftDeleteFilters(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rol>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Kullanici>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Poliklinik>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Hasta>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Doktor>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Randevu>().HasQueryFilter(e => !e.IsDeleted);
        }

        private static void ConfigureRelationships(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Kullanici>()
                .HasOne(k => k.Rol)
                .WithMany(r => r.Kullanicilar)
                .HasForeignKey(k => k.RolId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Doktor>()
                .HasOne(d => d.Poliklinik)
                .WithMany(p => p.Doktorlar)
                .HasForeignKey(d => d.PoliklinikId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Hasta)
                .WithMany(h => h.Randevular)
                .HasForeignKey(r => r.HastaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Randevu>()
                .HasOne(r => r.Doktor)
                .WithMany(d => d.Randevular)
                .HasForeignKey(r => r.DoktorId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        private List<AuditLog> CreateAuditLogs()
        {
            ChangeTracker.DetectChanges();
            var auditLogs = new List<AuditLog>();
            var islemTarihi = DateTime.UtcNow;

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog || entry.Entity is not BaseEntity)
                {
                    continue;
                }

                if (entry.State is EntityState.Detached or EntityState.Unchanged)
                {
                    continue;
                }

                var tabloAdi = entry.Metadata.ClrType.Name;
                string? eskiDegerler = null;
                string? yeniDegerler = null;
                string islemTipi;

                switch (entry.State)
                {
                    case EntityState.Added:
                        islemTipi = "Ekleme";
                        yeniDegerler = SerializePropertyValues(entry.CurrentValues);
                        break;

                    case EntityState.Modified:
                        islemTipi = "Guncelleme";
                        var degisenAlanlar = entry.Properties
                            .Where(p => p.IsModified)
                            .Select(p => p.Metadata.Name)
                            .ToList();

                        if (degisenAlanlar.Count > 0)
                        {
                            eskiDegerler = SerializePropertyValues(entry.OriginalValues, degisenAlanlar);
                            yeniDegerler = SerializePropertyValues(entry.CurrentValues, degisenAlanlar);
                        }

                        break;

                    case EntityState.Deleted:
                        islemTipi = "Silme";
                        eskiDegerler = SerializePropertyValues(entry.OriginalValues);
                        break;

                    default:
                        continue;
                }

                auditLogs.Add(new AuditLog
                {
                    TabloAdi = tabloAdi,
                    IslemTipi = islemTipi,
                    KullaniciAdi = CurrentKullaniciAdi,
                    IslemTarihi = islemTarihi,
                    EskiDegerler = eskiDegerler,
                    YeniDegerler = yeniDegerler
                });
            }

            return auditLogs;
        }

        private static string? SerializePropertyValues(
            PropertyValues values,
            IEnumerable<string>? propertyNames = null)
        {
            var alanlar = propertyNames?.ToHashSet();
            var dict = new Dictionary<string, object?>();

            foreach (var property in values.Properties)
            {
                if (alanlar is not null && !alanlar.Contains(property.Name))
                {
                    continue;
                }

                dict[property.Name] = values[property.Name];
            }

            return dict.Count == 0 ? null : JsonSerializer.Serialize(dict, JsonOptions);
        }
    }
}
