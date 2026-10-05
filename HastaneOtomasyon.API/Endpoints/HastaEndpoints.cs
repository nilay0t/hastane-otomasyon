using HastaneOtomasyon.Data.Context;
using HastaneOtomasyon.Data.Entities;
using HastaneOtomasyon.Data.Enums;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Hasta;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.API.Endpoints
{
    public static class HastaEndpoints
    {
        public static IEndpointRouteBuilder MapHastaEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/hastalar");

            group.MapGet("/", GetAllAsync);
            group.MapGet("/ara", AraAsync);
            group.MapGet("/{id:int}", GetByIdAsync);
            group.MapPost("/", CreateAsync);

            return routes;
        }

        private static async Task<IResult> GetAllAsync(AppDbContext context)
        {
            var hastalar = (await context.Hastalar
                .OrderBy(h => h.AdSoyad)
                .ToListAsync())
                .Select(MapToDto)
                .ToList();

            return Results.Ok(new ApiResponse<List<HastaDto>>(
                true,
                "Hastalar basariyla listelendi.",
                hastalar));
        }

        private static async Task<IResult> GetByIdAsync(
            int id,
            AppDbContext context)
        {
            if (id <= 0)
            {
                return Results.BadRequest(new ApiResponse<HastaDto>(
                    false,
                    "Gecerli bir hasta id degeri giriniz."));
            }

            var hastaEntity = await context.Hastalar
                .FirstOrDefaultAsync(h => h.Id == id);

            var hasta = hastaEntity is null ? null : MapToDto(hastaEntity);

            if (hasta is null)
            {
                return Results.NotFound(new ApiResponse<HastaDto>(
                    false,
                    "Hasta bulunamadi."));
            }

            return Results.Ok(new ApiResponse<HastaDto>(
                true,
                "Hasta detayi getirildi.",
                hasta));
        }

        private static async Task<IResult> AraAsync(
            string? ad,
            string? tc,
            AppDbContext context)
        {
            if (string.IsNullOrWhiteSpace(ad) && string.IsNullOrWhiteSpace(tc))
            {
                return Results.BadRequest(new ApiResponse<List<HastaDto>>(
                    false,
                    "Arama icin ad veya tc parametresinden en az biri zorunludur."));
            }

            var hastalar = (await context.Hastalar
                .Where(h =>
                    (!string.IsNullOrWhiteSpace(ad) && h.AdSoyad.Contains(ad)) ||
                    (!string.IsNullOrWhiteSpace(tc) && h.TcKimlikNo == tc))
                .OrderBy(h => h.AdSoyad)
                .ToListAsync())
                .Select(MapToDto)
                .ToList();

            return Results.Ok(new ApiResponse<List<HastaDto>>(
                true,
                "Arama sonuclari getirildi.",
                hastalar));
        }

        private static async Task<IResult> CreateAsync(
            HastaDto request,
            string? kullaniciAdi,
            AppDbContext context)
        {
            AtamaKullanici(context, kullaniciAdi);

            var validasyonHatasi = ValidateHastaDto(request, yeniKayit: true);
            if (validasyonHatasi is not null)
            {
                return Results.BadRequest(new ApiResponse<HastaDto>(
                    false,
                    validasyonHatasi));
            }

            if (!Enum.TryParse<Cinsiyet>(request.Cinsiyet, true, out var cinsiyet))
            {
                return Results.BadRequest(new ApiResponse<HastaDto>(
                    false,
                    "Gecersiz cinsiyet degeri. Erkek veya Kadin olmalidir."));
            }

            if (!request.KullaniciId.HasValue || request.KullaniciId.Value <= 0)
            {
                return Results.BadRequest(new ApiResponse<HastaDto>(
                    false,
                    "KullaniciId zorunludur."));
            }

            var tcMevcut = await context.Hastalar
                .AnyAsync(h => h.TcKimlikNo == request.TcKimlikNo);

            if (tcMevcut)
            {
                return Results.BadRequest(new ApiResponse<HastaDto>(
                    false,
                    "Bu TC kimlik numarasi zaten kayitli."));
            }

            var hasta = new Hasta
            {
                TcKimlikNo = request.TcKimlikNo,
                AdSoyad = request.AdSoyad,
                DogumTarihi = request.DogumTarihi,
                Cinsiyet = cinsiyet,
                Telefon = request.Telefon,
                Email = request.Email,
                Adres = request.Adres,
                KullaniciId = request.KullaniciId.Value,
                CreatedAt = DateTime.UtcNow
            };

            context.Hastalar.Add(hasta);
            await context.SaveChangesAsync();

            var dto = MapToDto(hasta);

            return Results.Ok(new ApiResponse<HastaDto>(
                true,
                "Hasta basariyla olusturuldu.",
                dto));
        }

        private static HastaDto MapToDto(Hasta hasta)
        {
            return new HastaDto
            {
                Id = hasta.Id,
                TcKimlikNo = hasta.TcKimlikNo,
                AdSoyad = hasta.AdSoyad,
                DogumTarihi = hasta.DogumTarihi,
                Cinsiyet = hasta.Cinsiyet.ToString(),
                Telefon = hasta.Telefon,
                Email = hasta.Email,
                Adres = hasta.Adres,
                KullaniciId = hasta.KullaniciId
            };
        }

        private static string? ValidateHastaDto(HastaDto request, bool yeniKayit)
        {
            if (string.IsNullOrWhiteSpace(request.TcKimlikNo) ||
                request.TcKimlikNo.Length != 11 ||
                !request.TcKimlikNo.All(char.IsDigit))
            {
                return "TC kimlik numarasi 11 haneli ve sayisal olmalidir.";
            }

            if (string.IsNullOrWhiteSpace(request.AdSoyad))
            {
                return "Ad soyad zorunludur.";
            }

            if (string.IsNullOrWhiteSpace(request.Telefon))
            {
                return "Telefon zorunludur.";
            }

            if (string.IsNullOrWhiteSpace(request.Cinsiyet))
            {
                return "Cinsiyet zorunludur.";
            }

            if (request.DogumTarihi == default)
            {
                return "Dogum tarihi zorunludur.";
            }

            if (!yeniKayit && request.Id <= 0)
            {
                return "Gecerli bir hasta id degeri giriniz.";
            }

            return null;
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
