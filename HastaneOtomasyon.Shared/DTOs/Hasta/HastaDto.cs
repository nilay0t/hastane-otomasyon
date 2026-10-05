namespace HastaneOtomasyon.Shared.DTOs.Hasta
{
    public class HastaDto
    {
        public required int Id { get; set; }
        public required string TcKimlikNo { get; set; }
        public required string AdSoyad { get; set; }
        public required DateTime DogumTarihi { get; set; }
        public required string Cinsiyet { get; set; }
        public required string Telefon { get; set; }
        public string? Email { get; set; }
        public string? Adres { get; set; }
        public int? KullaniciId { get; set; }
    }
}