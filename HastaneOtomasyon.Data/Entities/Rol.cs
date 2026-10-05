namespace HastaneOtomasyon.Data.Entities
{
    public class Rol : BaseEntity
    {
        public string RolAdi { get; set; } = string.Empty;

        public string? Aciklama { get; set; }

        public ICollection<Kullanici> Kullanicilar { get; set; } = new List<Kullanici>();
    }
}
