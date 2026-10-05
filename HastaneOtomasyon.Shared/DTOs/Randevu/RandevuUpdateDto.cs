namespace HastaneOtomasyon.Shared.DTOs.Randevu
{
    public class RandevuUpdateDto
    {
        public required int HastaId { get; set; }
        public required int DoktorId { get; set; }
        public required DateTime RandevuTarihi { get; set; }
        public required string Durum { get; set; }
        public string? Notlar { get; set; }
        public string? RandevuTuru { get; set; }
    }
}