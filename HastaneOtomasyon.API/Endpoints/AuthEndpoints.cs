using HastaneOtomasyon.Data.Context;
using HastaneOtomasyon.Data.Entities;
using HastaneOtomasyon.Shared.DTOs.Auth;
using HastaneOtomasyon.Shared.DTOs.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.API.Endpoints
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/auth");

            group.MapPost("/login", LoginAsync);
            group.MapPost("/register", RegisterAsync);
            group.MapPost("/change-password", ChangePasswordAsync);
            group.MapPost("/logout", LogoutAsync);

            return routes;
        }

        private static async Task<IResult> LoginAsync(
            LoginRequest request,
            AppDbContext context)
        {
            if (string.IsNullOrWhiteSpace(request.KullaniciAdi) ||
                string.IsNullOrWhiteSpace(request.Sifre))
            {
                return Results.BadRequest(new ApiResponse<LoginResponse>(
                    false,
                    "Kullanici adi ve sifre zorunludur."));
            }

            var kullanici = await context.Kullanicilar
                .Include(k => k.Rol)
                .FirstOrDefaultAsync(k =>
                    k.KullaniciAdi == request.KullaniciAdi &&
                    k.Sifre == request.Sifre);

            if (kullanici is null)
            {
                return Results.Json(
                    new ApiResponse<LoginResponse>(false, "Kullanici adi veya sifre hatali."),
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            kullanici.SonGirisTarihi = DateTime.UtcNow;
            context.CurrentKullaniciAdi = kullanici.KullaniciAdi;
            await context.SaveChangesAsync();

            // Role göre Hasta / Doktor entity'sinin Id'sini bul
            int? hastaId = null;
            int? doktorId = null;

            if (kullanici.Rol.RolAdi == "Hasta")
            {
                var hasta = await context.Hastalar
                    .FirstOrDefaultAsync(h => h.KullaniciId == kullanici.Id);
                hastaId = hasta?.Id;
            }
            else if (kullanici.Rol.RolAdi == "Doktor")
            {
                var doktor = await context.Doktorlar
                    .FirstOrDefaultAsync(d => d.KullaniciId == kullanici.Id);
                doktorId = doktor?.Id;
            }

            var loginResponse = new LoginResponse
            {
                Token = $"simulated-jwt-{Guid.NewGuid():N}",
                KullaniciAdi = kullanici.KullaniciAdi,
                Rol = kullanici.Rol.RolAdi,
                HastaId = hastaId,
                DoktorId = doktorId
            };

            return Results.Ok(new ApiResponse<LoginResponse>(
                true,
                "Giris basarili.",
                loginResponse));
        }

        private static async Task<IResult> RegisterAsync(
            RegisterRequest request,
            AppDbContext context)
        {
            if (string.IsNullOrWhiteSpace(request.KullaniciAdi) ||
                string.IsNullOrWhiteSpace(request.Sifre) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.AdSoyad))
            {
                return Results.BadRequest(new ApiResponse<object>(
                    false,
                    "Kullanici adi, sifre, e-posta ve ad soyad zorunludur."));
            }

            var mevcutKullanici = await context.Kullanicilar
                .AnyAsync(k =>
                    k.KullaniciAdi == request.KullaniciAdi ||
                    k.Email == request.Email);

            if (mevcutKullanici)
            {
                return Results.BadRequest(new ApiResponse<object>(
                    false,
                    "Bu kullanici adi veya e-posta adresi zaten kayitli."));
            }

            var hastaRol = await context.Roller
                .FirstOrDefaultAsync(r => r.RolAdi == "Hasta");

            if (hastaRol is null)
            {
                return Results.Json(
                    new ApiResponse<object>(false, "Hasta rolu bulunamadi."),
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            var yeniKullanici = new Kullanici
            {
                KullaniciAdi = request.KullaniciAdi,
                Sifre = request.Sifre,
                Email = request.Email,
                AdSoyad = request.AdSoyad,
                RolId = hastaRol.Id,
                CreatedAt = DateTime.UtcNow
            };

            context.Kullanicilar.Add(yeniKullanici);
            context.CurrentKullaniciAdi = request.KullaniciAdi;
            await context.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object>(
                true,
                "Kayit islemi basarili.",
                new { yeniKullanici.Id, yeniKullanici.KullaniciAdi }));
        }

        private static async Task<IResult> ChangePasswordAsync(
            SifreDegistirRequest request,
            string? kullaniciAdi,
            AppDbContext context)
        {
            if (string.IsNullOrWhiteSpace(request.EskiSifre) ||
                string.IsNullOrWhiteSpace(request.YeniSifre))
            {
                return Results.BadRequest(new ApiResponse<object>(
                    false,
                    "Eski sifre ve yeni sifre zorunludur."));
            }

            var hedefKullaniciAdi = string.IsNullOrWhiteSpace(kullaniciAdi)
                ? context.CurrentKullaniciAdi
                : kullaniciAdi;

            if (string.IsNullOrWhiteSpace(hedefKullaniciAdi) ||
                hedefKullaniciAdi == "Sistem")
            {
                return Results.BadRequest(new ApiResponse<object>(
                    false,
                    "Kullanici adi belirtilmelidir (query: kullaniciAdi)."));
            }

            var kullanici = await context.Kullanicilar
                .FirstOrDefaultAsync(k => k.KullaniciAdi == hedefKullaniciAdi);

            if (kullanici is null)
            {
                return Results.NotFound(new ApiResponse<object>(
                    false,
                    "Kullanici bulunamadi."));
            }

            if (kullanici.Sifre != request.EskiSifre)
            {
                return Results.BadRequest(new ApiResponse<object>(
                    false,
                    "Eski sifre hatali."));
            }

            kullanici.Sifre = request.YeniSifre;
            kullanici.UpdatedAt = DateTime.UtcNow;
            context.CurrentKullaniciAdi = kullanici.KullaniciAdi;
            await context.SaveChangesAsync();

            return Results.Ok(new ApiResponse<object>(
                true,
                "Sifre basariyla guncellendi."));
        }

        private static IResult LogoutAsync()
        {
            return Results.Ok(new ApiResponse<object>(
                true,
                "Cikis islemi basarili."));
        }
    }
}
