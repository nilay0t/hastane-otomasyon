## Genel Kurallar Tablosu

| Yapı | Kural | Örnek |
|---|---|---|
| Class / Interface | PascalCase | `RandevuService`, `IAuthService` |
| Public Method | PascalCase | `GetAllAsync()`, `CreateAsync()` |
| Public Property | PascalCase | `RandevuTarihi`, `AdSoyad` |
| Private Field | `_camelCase` | `_httpClient`, `_dbContext` |
| Local Variable | camelCase | `randevuListesi`, `kullaniciAdi` |
| Parameter | camelCase | `int hastaId`, `string tcKimlikNo` |
| Constant | PascalCase | `const int MaxRandevuSayisi = 10;` |
| Enum | PascalCase | `RandevuDurumu`, `Cinsiyet` |
| Enum Değeri | PascalCase | `Bekliyor`, `Onaylandi`, `IptalEdildi` |
| Interface | `I` + PascalCase | `IAuthService`, `IRandevuService` |
| Abstract Class | PascalCase | `BaseEntity` |
| DTO Class | PascalCase + `Dto` | `RandevuCreateDto`, `HastaDto` |
| XAML Dosya | PascalCase + `Page` | `LoginPage.xaml`, `RandevuListPage.xaml` |
| Namespace | PascalCase | `HastaneOtomasyon.Data.Entities` |
| Async Method | PascalCase + `Async` | `LoginAsync()`, `GetAllAsync()` |

## Genel Kurallar

- **Türkçe karakter** (I, Ş, Ü, Ö, Ç, Ğ) class, method ve değişken adlarında **kullanılmaz**
- Entity property'lerinde Türkçe **kullanılabilir** (`AdSoyad`, `DogumTarihi`) ama class adında **değil**
- Her dosyanın başında gerekli `using` ifadeleri olmalı, **gereksizler silinmiş**
- Bir satır boşluk: method'lar arasında
- İki satır boşluk: section'lar arasında
- Süslü parantez `{` her zaman **yeni satırda** başlar (Allman style)

## Doğru / Yanlış Örnekleri

```csharp
// doğru ifadeler
public class RandevuService : IRandevuService
{
    private readonly HttpClient _httpClient;

    public async Task<ApiResponse<List<RandevuListDto>>> GetAllAsync()
    {
        var randevuListesi = await _httpClient.GetFromJsonAsync<...>("randevular");
        return randevuListesi;
    }
}

//yanlış ifadeler
public class randevuService : iAuthService    // class lowercase, interface I yok
{
    private HttpClient HttpClient;             // _ ve readonly yok, PascalCase yanlış

    public async Task<ApiResponse<List<RandevuListDto>>> GetAll() {    // Async eki yok, { aynı satırda
        var RandevuListesi = ...;              // local variable PascalCase yanlış
    }
}