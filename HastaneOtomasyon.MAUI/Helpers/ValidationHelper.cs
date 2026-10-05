using System.Text.RegularExpressions;

namespace HastaneOtomasyon.MAUI.Helpers
{
    public static class ValidationHelper
    {
        // Boş veya beyaz boşluk kontrol
        public static bool IsBosMu(string? deger)
        {
            return string.IsNullOrWhiteSpace(deger);
        }

      
        public static bool IsEmailGecerli(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(email, pattern);
        }

        // TC kimlik 11 haneli ve sadece rakam 
        public static bool IsTcKimlikGecerli(string? tc)
        {
            if (string.IsNullOrWhiteSpace(tc)) return false;
            if (tc.Length != 11) return false;
            return tc.All(char.IsDigit);
        }

        // Telefon en az 10 haneli rakam 
        public static bool IsTelefonGecerli(string? telefon)
        {
            if (string.IsNullOrWhiteSpace(telefon)) return false;
            var rakamlar = telefon.Where(char.IsDigit).Count();
            return rakamlar >= 10;
        }

       
        public static bool IsSifreGuclu(string? sifre)
        {
            if (string.IsNullOrWhiteSpace(sifre)) return false;
            if (sifre.Length < 6) return false;
            bool harfVar = sifre.Any(char.IsLetter);
            bool rakamVar = sifre.Any(char.IsDigit);
            return harfVar && rakamVar;
        }
        // Geçmiş tarihte randevu alamaz

        public static bool IsGecmisTarih(DateTime tarih)
        {
            return tarih < DateTime.Now;
        }
    }
}