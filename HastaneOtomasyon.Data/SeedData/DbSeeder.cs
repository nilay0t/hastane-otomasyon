using HastaneOtomasyon.Data.Context;
using HastaneOtomasyon.Data.Entities;
using HastaneOtomasyon.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.Data.SeedData
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            var seedTarihi = DateTime.UtcNow;

            if (!await context.Roller.AnyAsync())
            {
                context.Roller.AddRange(
                    new Rol
                    {
                        RolAdi = "Admin",
                        Aciklama = "Sistem yoneticisi",
                        CreatedAt = seedTarihi
                    },
                    new Rol
                    {
                        RolAdi = "Doktor",
                        Aciklama = "Doktor kullanicisi",
                        CreatedAt = seedTarihi
                    },
                    new Rol
                    {
                        RolAdi = "Hasta",
                        Aciklama = "Hasta kullanicisi",
                        CreatedAt = seedTarihi
                    });

                await context.SaveChangesAsync();
            }

            // Admin, Doktor, Hasta rollerini al
            var adminRol = await context.Roller.FirstAsync(r => r.RolAdi == "Admin");
            var doktorRol = await context.Roller.FirstAsync(r => r.RolAdi == "Doktor");
            var hastaRol = await context.Roller.FirstAsync(r => r.RolAdi == "Hasta");

            // admin kullanicisi
            if (!await context.Kullanicilar.AnyAsync(k => k.KullaniciAdi == "admin"))
            {
                context.Kullanicilar.Add(new Kullanici
                {
                    KullaniciAdi = "admin",
                    Sifre = "Admin123!",
                    Email = "admin@hastane.com",
                    AdSoyad = "Sistem Yöneticisi",
                    RolId = adminRol.Id,
                    CreatedAt = seedTarihi
                });
            }

            // doktor1 kullanicisi
            if (!await context.Kullanicilar.AnyAsync(k => k.KullaniciAdi == "doktor1"))
            {
                context.Kullanicilar.Add(new Kullanici
                {
                    KullaniciAdi = "doktor1",
                    Sifre = "Doktor123!",
                    Email = "doktor1@hastane.com",
                    AdSoyad = "Test Doktor",
                    RolId = doktorRol.Id,
                    CreatedAt = seedTarihi
                });
            }

            // hasta1 kullanicisi
            if (!await context.Kullanicilar.AnyAsync(k => k.KullaniciAdi == "hasta1"))
            {
                context.Kullanicilar.Add(new Kullanici
                {
                    KullaniciAdi = "hasta1",
                    Sifre = "Hasta123!",
                    Email = "hasta1@hastane.com",
                    AdSoyad = "Test Hasta",
                    RolId = hastaRol.Id,
                    CreatedAt = seedTarihi
                });
            }

            await context.SaveChangesAsync();

            if (!await context.Poliklinikler.AnyAsync())
            {
                context.Poliklinikler.AddRange(
                    new Poliklinik
                    {
                        PoliklinikAdi = "Dahiliye",
                        Aciklama = "Ic hastaliklari poliklinigi",
                        CreatedAt = seedTarihi
                    },
                    new Poliklinik
                    {
                        PoliklinikAdi = "Goz",
                        Aciklama = "Goz hastaliklari poliklinigi",
                        CreatedAt = seedTarihi
                    },
                    new Poliklinik
                    {
                        PoliklinikAdi = "KBB",
                        Aciklama = "Kulak burun bogaz poliklinigi",
                        CreatedAt = seedTarihi
                    });

                await context.SaveChangesAsync();
            }

            // doktor1 kullanicisi icin Doktor entity'si
            var doktor1Kullanici = await context.Kullanicilar
                .FirstOrDefaultAsync(k => k.KullaniciAdi == "doktor1");

            if (doktor1Kullanici != null &&
                !await context.Doktorlar.AnyAsync(d => d.KullaniciId == doktor1Kullanici.Id))
            {
                var ilkPoliklinik = await context.Poliklinikler.FirstAsync();

                context.Doktorlar.Add(new Doktor
                {
                    AdSoyad = "Test Doktor",
                    Uzmanlik = "Dahiliye",
                    PoliklinikId = ilkPoliklinik.Id,
                    KullaniciId = doktor1Kullanici.Id,
                    CreatedAt = seedTarihi
                });

                await context.SaveChangesAsync();
            }

            // hasta1 kullanicisi icin Hasta entity'si
            var hasta1Kullanici = await context.Kullanicilar
                .FirstOrDefaultAsync(k => k.KullaniciAdi == "hasta1");

            if (hasta1Kullanici != null &&
                !await context.Hastalar.AnyAsync(h => h.KullaniciId == hasta1Kullanici.Id))
            {
                context.Hastalar.Add(new Hasta
                {
                    TcKimlikNo = "12345678901",
                    AdSoyad = "Test Hasta",
                    DogumTarihi = new DateTime(1990, 1, 1),
                    Cinsiyet = Cinsiyet.Erkek,
                    Telefon = "5551234567",
                    Email = "hasta1@hastane.com",
                    KullaniciId = hasta1Kullanici.Id,
                    CreatedAt = seedTarihi
                });

                await context.SaveChangesAsync();
            }
        }
    }
}
