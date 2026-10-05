namespace HastaneOtomasyon.Shared.DTOs.Auth
{   public class RegisterRequest
    {
        public required string KullaniciAdi { get; set; }
        public required string Sifre { get; set; }
        public required string Email { get; set; }
        public required string AdSoyad { get; set; }
        public required int RolId { get; set; }
    }
}