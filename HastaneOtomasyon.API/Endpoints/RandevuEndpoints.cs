using HastaneOtomasyon.Data.Context;
using HastaneOtomasyon.Data.Entities;
using HastaneOtomasyon.Data.Enums;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Randevu;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.API.Endpoints
{
    public static class RandevuEndpoints
    {
        public static IEndpointRouteBuilder MapRandevuEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/randevular");

            group.MapGet("/", GetAllAsync);
            group.MapGet("/tarih", GetByTarihAraligiAsync);
            group.MapGet("/hasta/{hastaId:int}", GetByHastaAsync);
            group.MapGet("/doktor/{doktorId:int}", GetByDoktorAsync);
            group.MapGet("/{id:int}", GetByIdAsync);
            group.MapPost("/", CreateAsync);
            group.MapPut("/{id:int}", UpdateAsync);
            group.MapDelete("/{id:int}", DeleteAsync);

            return routes;
        }

        private static async Task<IResult> GetAllAsync(AppDbContext context)
        {
            var randevular = await MapRandevuListQuery(context).ToListAsync();

            return Results.Ok(new ApiResponse<List<RandevuListDto>>(
                true,
                "Randevular basariyla listelendi.",
                randevular));
        }

        private static async Task<IResult> GetByIdAsync(
            int id,
            AppDbContext context)
        {
            if (id <= 0)
            {
                return Results.BadRequest(new ApiResponse<RandevuListDto>(
                    false,
                    "Gecerli bir randevu id degeri giriniz."));
            }

            var randevu = await MapRandevuListQuery(context)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (randevu is null)
            {
                return Results.NotFound(new ApiResponse<RandevuListDto>(
                    false,
                    "Randevu bulunamadi."));
            }

            return Results.Ok(new ApiResponse<RandevuListDto>(
                true,
                "Randevu detayi getirildi.",
                randevu));
        }

        private static async Task<IResult> GetByHastaAsync(
            int hastaId,
            AppDbContext context)
        {
            if (hastaId <= 0)
            {
                return Results.BadRequest(new ApiResponse<List<RandevuListDto>>(
                    false,
                    "Gecerli bir hasta id degeri giriniz."));
            }

            var randevular = await MapRandevuListQuery(context)
                .Where(r => r.HastaId == hastaId)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<RandevuListDto>>(
                true,
                "Hastaya ait randevular listelendi.",
                randevular));
        }

        private static async Task<IResult> GetByDoktorAsync(
            int doktorId,
            AppDbContext context)
        {
            if (doktorId <= 0)
            {
                return Results.BadRequest(new ApiResponse<List<RandevuListDto>>(
                    false,
                    "Gecerli bir doktor id degeri giriniz."));
            }

            var randevular = await MapRandevuListQuery(context)
                .Where(r => r.DoktorId == doktorId)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<RandevuListDto>>(
                true,
                "Doktora ait randevular listelendi.",
                randevular));
        }

        private static async Task<IResult> GetByTarihAraligiAsync(
            DateTime? baslangic,
            DateTime? bitis,
            AppDbContext context)
        {
            if (!baslangic.HasValue || !bitis.HasValue)
            {
                return Results.BadRequest(new ApiResponse<List<RandevuListDto>>(
                    false,
                    "baslangic ve bitis tarih parametreleri zorunludur."));
            }

            if (baslangic.Value > bitis.Value)
            {
                return Results.BadRequest(new ApiResponse<List<RandevuListDto>>(
                    false,
                    "baslangic tarihi bitis tarihinden buyuk olamaz."));
            }

            var randevular = await MapRandevuListQuery(context)
                .Where(r =>
                    r.RandevuTarihi >= baslangic.Value &&
                    r.RandevuTarihi <= bitis.Value)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<RandevuListDto>>(
                true,
                "Tarih araligindaki randevular listelendi.",
                randevular));
        }

        private static async Task<IResult> CreateAsync(
            RandevuCreateDto request,
            string? kullaniciAdi,
            AppDbContext context)
        {
            AtamaKullanici(context, kullaniciAdi);

            if (request.HastaId <= 0 || request.DoktorId <= 0)
            {
                return Results.BadRequest(new ApiResponse<RandevuListDto>(
                    false,
                    "HastaId ve DoktorId zorunludur."));
            }

            if (request.RandevuTarihi == default)
            {
                return Results.BadRequest(new ApiResponse<RandevuListDto>(
                    false,
                    "Randevu tarihi zorunludur."));
            }

            var hastaVar = await context.Hastalar.AnyAsync(h => h.Id == request.HastaId);
            var doktorVar = await context.Doktorlar.AnyAsync(d => d.Id == request.DoktorId);

            if (!hastaVar || !doktorVar)
            {
                return Results.NotFound(new ApiResponse<RandevuListDto>(
                    false,
                    "Hasta veya doktor bulunamadi."));
            }

            if (await HasRandevuCakismasiAsync(context, request.DoktorId, request.RandevuTarihi))
            {
                return Results.Json(
                    new ApiResponse<RandevuListDto>(
                        false,
                        "Bu doktor icin secilen tarih ve saatte aktif bir randevu zaten mevcut."),
                    statusCode: StatusCodes.Status409Conflict);
            }

            var randevu = new Randevu
            {
                HastaId = request.HastaId,
                DoktorId = request.DoktorId,
                RandevuTarihi = request.RandevuTarihi,
                Durum = RandevuDurumu.Bekliyor,
                Notlar = request.Notlar,
                RandevuTuru = request.RandevuTuru,
                CreatedAt = DateTime.UtcNow
            };

            context.Randevular.Add(randevu);
            await context.SaveChangesAsync();

            var dto = await MapRandevuListQuery(context)
                .FirstAsync(r => r.Id == randevu.Id);

            return Results.Ok(new ApiResponse<RandevuListDto>(
                true,
                "Randevu basariyla olusturuldu.",
                dto));
        }

        private static async Task<IResult> UpdateAsync(
            int id,
            RandevuUpdateDto request,
            string? kullaniciAdi,
            AppDbContext context)
        {
            AtamaKullanici(context, kullaniciAdi);

            if (id <= 0)
            {
                return Results.BadRequest(new ApiResponse<RandevuListDto>(
                    false,
                    "Gecerli bir randevu id degeri giriniz."));
            }

            if (request.HastaId <= 0 || request.DoktorId <= 0)
            {
                return Results.BadRequest(new ApiResponse<RandevuListDto>(
                    false,
                    "HastaId ve DoktorId zorunludur."));
            }

            if (request.RandevuTarihi == default)
            {
                return Results.BadRequest(new ApiResponse<RandevuListDto>(
                    false,
                    "Randevu tarihi zorunludur."));
            }

            if (!Enum.TryParse<RandevuDurumu>(request.Durum, true, out var durum))
            {
                return Results.BadRequest(new ApiResponse<RandevuListDto>(
                    false,
                    "Gecersiz randevu durumu."));
            }

            var randevu = await context.Randevular.FirstOrDefaultAsync(r => r.Id == id);

            if (randevu is null)
            {
                return Results.NotFound(new ApiResponse<RandevuListDto>(
                    false,
                    "Randevu bulunamadi."));
            }

            if (await HasRandevuCakismasiAsync(
                    context,
                    request.DoktorId,
                    request.RandevuTarihi,
                    haricRandevuId: id) &&
                (durum == RandevuDurumu.Bekliyor || durum == RandevuDurumu.Onaylandi))
            {
                return Results.Json(
                    new ApiResponse<RandevuListDto>(
                        false,
                        "Bu doktor icin secilen tarih ve saatte aktif bir randevu zaten mevcut."),
                    statusCode: StatusCodes.Status409Conflict);
            }

            randevu.HastaId = request.HastaId;
            randevu.DoktorId = request.DoktorId;
            randevu.RandevuTarihi = request.RandevuTarihi;
            randevu.Durum = durum;
            randevu.Notlar = request.Notlar;
            randevu.RandevuTuru = request.RandevuTuru;
            randevu.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            var dto = await MapRandevuListQuery(context)
                .FirstAsync(r => r.Id == randevu.Id);

            return Results.Ok(new ApiResponse<RandevuListDto>(
                true,
                "Randevu basariyla guncellendi.",
                dto));
        }

        private static async Task<IResult> DeleteAsync(
            int id,
            string? kullaniciAdi,
            AppDbContext context)
        {
            AtamaKullanici(context, kullaniciAdi);

            if (id <= 0)
            {
                return Results.BadRequest(new ApiResponse<object>(
                    false,
                    "Gecerli bir randevu id degeri giriniz."));
            }

            var randevu = await context.Randevular.FirstOrDefaultAsync(r => r.Id == id);

            if (randevu is null)
            {
                return Results.NotFound(new ApiResponse<object>(
                    false,
                    "Randevu bulunamadi."));
            }

            randevu.IsDeleted = true;
            randevu.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object>(
                true,
                "Randevu basariyla silindi (soft delete)."));
        }

        private static IQueryable<RandevuListDto> MapRandevuListQuery(AppDbContext context)
        {
            return context.Randevular
                .Include(r => r.Hasta)
                .Include(r => r.Doktor)
                .ThenInclude(d => d.Poliklinik)
                .OrderByDescending(r => r.RandevuTarihi)
                .Select(r => new RandevuListDto
                {
                    Id = r.Id,
                    HastaId = r.HastaId,
                    HastaAdSoyad = r.Hasta.AdSoyad,
                    DoktorId = r.DoktorId,
                    DoktorAdSoyad = r.Doktor.AdSoyad,
                    PoliklinikAdi = r.Doktor.Poliklinik.PoliklinikAdi,
                    RandevuTarihi = r.RandevuTarihi,
                    Durum = r.Durum.ToString(),
                    Notlar = r.Notlar,
                    RandevuTuru = r.RandevuTuru
                });
        }

        private static async Task<bool> HasRandevuCakismasiAsync(
            AppDbContext context,
            int doktorId,
            DateTime randevuTarihi,
            int? haricRandevuId = null)
        {
            var query = context.Randevular.Where(r =>
                r.DoktorId == doktorId &&
                r.RandevuTarihi == randevuTarihi &&
                (r.Durum == RandevuDurumu.Bekliyor || r.Durum == RandevuDurumu.Onaylandi));

            if (haricRandevuId.HasValue)
            {
                query = query.Where(r => r.Id != haricRandevuId.Value);
            }

            return await query.AnyAsync();
        }

        private static void AtamaKullanici(AppDbContext context, string? kullaniciAdi)
        {
            if (!string.IsNullOrWhiteSpace(kullaniciAdi))
            {
                context.CurrentKullaniciAdi = kullaniciAdi;
            }
        }
    }
}
