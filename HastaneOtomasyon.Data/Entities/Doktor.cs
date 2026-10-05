namespace HastaneOtomasyon.Data.Entities
{
    public class Doktor : BaseEntity
    {
        public string AdSoyad { get; set; } = string.Empty;

        public string Uzmanlik { get; set; } = string.Empty;

        public int PoliklinikId { get; set; }

        public int KullaniciId { get; set; }

        public Poliklinik Poliklinik { get; set; } = null!;

        public ICollection<Randevu> Randevular { get; set; } = new List<Randevu>();
    }
}
