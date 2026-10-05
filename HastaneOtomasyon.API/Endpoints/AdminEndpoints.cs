using HastaneOtomasyon.Data.Context;
using HastaneOtomasyon.Data.Enums;
using HastaneOtomasyon.Shared.DTOs.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.API.Endpoints
{
    public static class AdminEndpoints
    {
        public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/admin");

            group.MapGet("/auditlogs", GetAuditLogsAsync);
            group.MapGet("/istatistik", GetIstatistikAsync);

            return routes;
        }

        private static async Task<IResult> GetAuditLogsAsync(
            string? kullaniciAdi,
            AppDbContext context)
        {
            AtamaKullanici(context, kullaniciAdi);

            var auditLogs = await context.AuditLogs
                .OrderByDescending(a => a.IslemTarihi)
                .Select(a => new AuditLogDto
                {
                    Id = a.Id,
                    TabloAdi = a.TabloAdi,
                    IslemTipi = a.IslemTipi,
                    KullaniciAdi = a.KullaniciAdi,
                    IslemTarihi = a.IslemTarihi,
                    EskiDegerler = a.EskiDegerler,
                    YeniDegerler = a.YeniDegerler
                })
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<AuditLogDto>>(
                true,
                "Audit log kayitlari listelendi.",
                auditLogs));
        }

        private static async Task<IResult> GetIstatistikAsync(
            string? kullaniciAdi,
            AppDbContext context)
        {
            AtamaKullanici(context, kullaniciAdi);

            var toplamAktifRandevu = await context.Randevular
                .CountAsync(r =>
                    r.Durum == RandevuDurumu.Bekliyor ||
                    r.Durum == RandevuDurumu.Onaylandi);

            var poliklinikDagilimi = await context.Randevular
                .Include(r => r.Doktor)
                .ThenInclude(d => d.Poliklinik)
                .GroupBy(r => new
                {
                    r.Doktor.PoliklinikId,
                    r.Doktor.Poliklinik.PoliklinikAdi
                })
                .Select(g => new PoliklinikRandevuDagilimi
                {
                    PoliklinikId = g.Key.PoliklinikId,
                    PoliklinikAdi = g.Key.PoliklinikAdi,
                    RandevuSayisi = g.Count()
                })
                .OrderByDescending(p => p.RandevuSayisi)
                .ToListAsync();

            var enCokRandevuAlanHastalar = await context.Randevular
                .Include(r => r.Hasta)
                .GroupBy(r => new { r.HastaId, r.Hasta.AdSoyad })
                .Select(g => new EnCokRandevuHasta
                {
                    HastaId = g.Key.HastaId,
                    AdSoyad = g.Key.AdSoyad,
                    RandevuSayisi = g.Count()
                })
                .OrderByDescending(h => h.RandevuSayisi)
                .Take(3)
                .ToListAsync();

            var rapor = new IstatistikRaporDto
            {
                ToplamAktifRandevuSayisi = toplamAktifRandevu,
                PoliklinikRandevuDagilimi = poliklinikDagilimi,
                EnCokRandevuAlanHastalar = enCokRandevuAlanHastalar
            };

            return Results.Ok(new ApiResponse<IstatistikRaporDto>(
                true,
                "Istatistik raporu olusturuldu.",
                rapor));
        }

        private static void AtamaKullanici(AppDbContext context, string? kullaniciAdi)
        {
            if (!string.IsNullOrWhiteSpace(kullaniciAdi))
            {
                context.CurrentKullaniciAdi = kullaniciAdi;
            }
        }
    }

    public class IstatistikRaporDto
    {
        public int ToplamAktifRandevuSayisi { get; set; }

        public List<PoliklinikRandevuDagilimi> PoliklinikRandevuDagilimi { get; set; } =
            new List<PoliklinikRandevuDagilimi>();

        public List<EnCokRandevuHasta> EnCokRandevuAlanHastalar { get; set; } =
            new List<EnCokRandevuHasta>();
    }

    public class PoliklinikRandevuDagilimi
    {
        public int PoliklinikId { get; set; }

        public string PoliklinikAdi { get; set; } = string.Empty;

        public int RandevuSayisi { get; set; }
    }

    public class EnCokRandevuHasta
    {
        public int HastaId { get; set; }

        public string AdSoyad { get; set; } = string.Empty;

        public int RandevuSayisi { get; set; }
    }
}
