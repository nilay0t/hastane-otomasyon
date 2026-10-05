namespace HastaneOtomasyon.Data.Entities
{
    public class Kullanici : BaseEntity
    {
        public string KullaniciAdi { get; set; } = string.Empty;

        public string Sifre { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string AdSoyad { get; set; } = string.Empty;

        public int RolId { get; set; }

        public DateTime? SonGirisTarihi { get; set; }

        public Rol Rol { get; set; } = null!;
    }
}
