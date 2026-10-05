

namespace HastaneOtomasyon.Shared.DTOs.Common
{
    public class AuditLogDto
    {
        public required int Id { get; set; }
        public required string TabloAdi { get; set; }
        public required string IslemTipi { get; set; }
        public required string KullaniciAdi { get; set; }
        public required DateTime IslemTarihi { get; set; }
        public string? EskiDegerler { get; set; }
        public string? YeniDegerler { get; set; }

    }
}
