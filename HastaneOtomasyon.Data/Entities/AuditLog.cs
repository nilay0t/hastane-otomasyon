namespace HastaneOtomasyon.Data.Entities
{
    public class AuditLog
    {
        public int Id { get; set; }

        public string TabloAdi { get; set; } = string.Empty;

        public string IslemTipi { get; set; } = string.Empty;

        public string KullaniciAdi { get; set; } = string.Empty;

        public DateTime IslemTarihi { get; set; }

        public string? EskiDegerler { get; set; }

        public string? YeniDegerler { get; set; }
    }
}
