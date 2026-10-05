using System.Net.Http.Json;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Randevu;

namespace HastaneOtomasyon.MAUI.Services.Implementations
{
    public class RandevuService : IRandevuService
    {
        private readonly HttpClient _httpClient;

        public RandevuService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<List<RandevuListDto>>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<List<RandevuListDto>>>("randevular");

                return response ?? new ApiResponse<List<RandevuListDto>>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<RandevuListDto>>(false, GetFriendlyErrorMessage(ex));
            }
        }

        public async Task<ApiResponse<RandevuListDto>> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<RandevuListDto>>($"randevular/{id}");

                return response ?? new ApiResponse<RandevuListDto>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<RandevuListDto>(false, GetFriendlyErrorMessage(ex));
            }
        }

        public async Task<ApiResponse<List<RandevuListDto>>> GetByHastaIdAsync(int hastaId)
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<List<RandevuListDto>>>(
                        $"randevular/hasta/{hastaId}");

                return response ?? new ApiResponse<List<RandevuListDto>>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<RandevuListDto>>(false, GetFriendlyErrorMessage(ex));
            }
        }

        public async Task<ApiResponse<List<RandevuListDto>>> GetByDoktorIdAsync(int doktorId)
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<List<RandevuListDto>>>(
                        $"randevular/doktor/{doktorId}");

                return response ?? new ApiResponse<List<RandevuListDto>>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<RandevuListDto>>(false, GetFriendlyErrorMessage(ex));
            }
        }

        public async Task<ApiResponse<List<RandevuListDto>>> GetByTarihAsync(DateTime bas, DateTime bit)
        {
            try
            {
                /*
                 * ÖNEMLİ:
                 * Eski kullanım:
                 *   bas.ToString("o")
                 *
                 * Bu format +03:00 gibi timezone bilgisi üretiyordu.
                 * URL içinde + işareti backend tarafında boşluk gibi algılanabiliyor.
                 * Bu yüzden backend şu hatayı veriyordu:
                 *   Failed to bind parameter "Nullable<DateTime> baslangic"
                 *
                 * Çözüm:
                 * Backend DateTime beklediği için timezone'suz, sade ISO format gönderiyoruz.
                 */
                var baslangic = Uri.EscapeDataString(bas.ToString("yyyy-MM-ddTHH:mm:ss"));
                var bitis = Uri.EscapeDataString(bit.ToString("yyyy-MM-ddTHH:mm:ss"));

                var url = $"randevular/tarih?baslangic={baslangic}&bitis={bitis}";

                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<List<RandevuListDto>>>(url);

                return response ?? new ApiResponse<List<RandevuListDto>>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<RandevuListDto>>(false, GetFriendlyErrorMessage(ex));
            }
        }

        public async Task<ApiResponse<bool>> CreateAsync(RandevuCreateDto dto)
        {
            try
            {
                var msg = await _httpClient.PostAsJsonAsync("randevular", dto);

                var raw = await msg.Content.ReadFromJsonAsync<ApiResponse<object>>();

                return new ApiResponse<bool>(
                    raw?.Success ?? msg.IsSuccessStatusCode,
                    raw?.Message ?? (msg.IsSuccessStatusCode ? "Randevu oluşturuldu" : "Randevu oluşturulamadı"));
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, GetFriendlyErrorMessage(ex));
            }
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, RandevuUpdateDto dto)
        {
            try
            {
                var msg = await _httpClient.PutAsJsonAsync($"randevular/{id}", dto);

                var raw = await msg.Content.ReadFromJsonAsync<ApiResponse<object>>();

                return new ApiResponse<bool>(
                    raw?.Success ?? msg.IsSuccessStatusCode,
                    raw?.Message ?? (msg.IsSuccessStatusCode ? "Randevu güncellendi" : "Randevu güncellenemedi"));
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, GetFriendlyErrorMessage(ex));
            }
        }

        public async Task<ApiResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var msg = await _httpClient.DeleteAsync($"randevular/{id}");

                var raw = await msg.Content.ReadFromJsonAsync<ApiResponse<object>>();

                return new ApiResponse<bool>(
                    raw?.Success ?? msg.IsSuccessStatusCode,
                    raw?.Message ?? (msg.IsSuccessStatusCode ? "Randevu iptal edildi" : "Randevu iptal edilemedi"));
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, GetFriendlyErrorMessage(ex));
            }
        }

        private static string GetFriendlyErrorMessage(Exception ex)
        {
            if (ex.Message.Contains("actively refused", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("bağlantı kurulamadı", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Hedef makine etkin olarak reddetti", StringComparison.OrdinalIgnoreCase))
            {
                return "Backend servisine bağlanılamadı. Lütfen API'nin çalıştığından emin olun.";
            }

            if (ex.Message.Contains("500", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Internal Server Error", StringComparison.OrdinalIgnoreCase))
            {
                return "Randevu işlemi sırasında backend tarafında bir hata oluştu.";
            }

            return ex.Message;
        }
    }
}