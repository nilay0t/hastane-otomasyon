using HastaneOtomasyon.Data.Enums;

namespace HastaneOtomasyon.Data.Entities
{
    public class Hasta : BaseEntity
    {
        public string TcKimlikNo { get; set; } = string.Empty;

        public string AdSoyad { get; set; } = string.Empty;

        public DateTime DogumTarihi { get; set; }

        public Cinsiyet Cinsiyet { get; set; }

        public string Telefon { get; set; } = string.Empty;

        public string? Email { get; set; }

        public string? Adres { get; set; }

        public int KullaniciId { get; set; }

        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
