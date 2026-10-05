using HastaneOtomasyon.Data.Context;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Doktor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.API.Endpoints
{
    public static class DoktorEndpoints
    {
        public static IEndpointRouteBuilder MapDoktorEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/doktorlar");

            group.MapGet("/", GetAllAsync);
            group.MapGet("/{id:int}", GetByIdAsync);

            return routes;
        }

        private static async Task<IResult> GetAllAsync(
            AppDbContext context,
            int? poliklinikId)
        {
            var query = context.Doktorlar
                .Include(d => d.Poliklinik)
                .AsQueryable();

            if (poliklinikId.HasValue)
            {
                if (poliklinikId.Value <= 0)
                {
                    return Results.BadRequest(new ApiResponse<List<DoktorDto>>(
                        false,
                        "Gecerli bir poliklinikId degeri giriniz."));
                }

                query = query.Where(d => d.PoliklinikId == poliklinikId.Value);
            }

            var doktorlar = await query
                .OrderBy(d => d.AdSoyad)
                .Select(d => new DoktorDto
                {
                    Id = d.Id,
                    AdSoyad = d.AdSoyad,
                    Uzmanlik = d.Uzmanlik,
                    PoliklinikId = d.PoliklinikId,
                    PoliklinikAdi = d.Poliklinik.PoliklinikAdi,
                    KullaniciId = d.KullaniciId
                })
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<DoktorDto>>(
                true,
                "Doktorlar basariyla listelendi.",
                doktorlar));
        }

        private static async Task<IResult> GetByIdAsync(
            int id,
            AppDbContext context)
        {
            if (id <= 0)
            {
                return Results.BadRequest(new ApiResponse<DoktorDto>(
                    false,
                    "Gecerli bir doktor id degeri giriniz."));
            }

            var doktor = await context.Doktorlar
                .Include(d => d.Poliklinik)
                .Where(d => d.Id == id)
                .Select(d => new DoktorDto
                {
                    Id = d.Id,
                    AdSoyad = d.AdSoyad,
                    Uzmanlik = d.Uzmanlik,
                    PoliklinikId = d.PoliklinikId,
                    PoliklinikAdi = d.Poliklinik.PoliklinikAdi,
                    KullaniciId = d.KullaniciId
                })
                .FirstOrDefaultAsync();

            if (doktor is null)
            {
                return Results.NotFound(new ApiResponse<DoktorDto>(
                    false,
                    "Doktor bulunamadi."));
            }

            return Results.Ok(new ApiResponse<DoktorDto>(
                true,
                "Doktor detayi getirildi.",
                doktor));
        }
    }
}
