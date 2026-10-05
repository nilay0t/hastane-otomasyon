using HastaneOtomasyon.Data.Enums;

namespace HastaneOtomasyon.Data.Entities
{
    public class Randevu : BaseEntity
    {
        public int HastaId { get; set; }

        public int DoktorId { get; set; }

        public DateTime RandevuTarihi { get; set; }

        public RandevuDurumu Durum { get; set; }

        public string? Notlar { get; set; }

        public string? RandevuTuru { get; set; }

        public Hasta Hasta { get; set; } = null!;

        public Doktor Doktor { get; set; } = null!;
    }
}
