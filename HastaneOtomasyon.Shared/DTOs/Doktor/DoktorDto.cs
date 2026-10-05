namespace HastaneOtomasyon.Shared.DTOs.Doktor
{
    public class DoktorDto
    {
        public required int Id { get; set; }
        public required string AdSoyad { get; set; }
        public required string Uzmanlik { get; set; }
        public required int PoliklinikId { get; set; }
        public required string PoliklinikAdi { get; set; }
        public required int KullaniciId { get; set; }

    }
}