namespace HastaneOtomasyon.Shared.DTOs.Auth
{
    public class LoginResponse
    {
        public required string Token { get; set; }
        public required string KullaniciAdi { get; set; }
        public required string Rol { get; set; }

        // Rol Hasta ise dolu, diğer rollerde null
        public int? HastaId { get; set; }

        // Rol Doktor ise dolu, diğer rollerde null
        public int? DoktorId { get; set; }
    }
}
