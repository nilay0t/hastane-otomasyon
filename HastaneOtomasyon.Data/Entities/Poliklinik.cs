namespace HastaneOtomasyon.Data.Entities
{
    public class Poliklinik : BaseEntity
    {
        public string PoliklinikAdi { get; set; } = string.Empty;

        public string? Aciklama { get; set; }

        public ICollection<Doktor> Doktorlar { get; set; } = new List<Doktor>();
    }
}
