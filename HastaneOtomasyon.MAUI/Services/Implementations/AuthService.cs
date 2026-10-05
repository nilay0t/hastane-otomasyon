using System.Net.Http.Json;
using HastaneOtomasyon.MAUI.Helpers;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Auth;

namespace HastaneOtomasyon.MAUI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        public AuthService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }


        public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/login", request);
                var apiResp = await response.Content
                    .ReadFromJsonAsync<ApiResponse<LoginResponse>>();

                if (apiResp is { Success: true, Data: not null })
                {
                    await SecureTokenStorage.SetAsync("auth_token", apiResp.Data.Token);
                    await SecureTokenStorage.SetAsync("kullanici_rol", apiResp.Data.Rol);
                    await SecureTokenStorage.SetAsync("kullanici_adi", apiResp.Data.KullaniciAdi);

                    // Rol Hasta ise HastaId, Rol Doktor ise DoktorId kaydet
                    if (apiResp.Data.HastaId.HasValue)
                    {
                        await SecureTokenStorage.SetAsync("hasta_id", apiResp.Data.HastaId.Value.ToString());
                    }

                    if (apiResp.Data.DoktorId.HasValue)
                    {
                        await SecureTokenStorage.SetAsync("doktor_id", apiResp.Data.DoktorId.Value.ToString());
                    }

                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiResp.Data.Token);
                }

                return apiResp ?? new ApiResponse<LoginResponse>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<LoginResponse>(false, ex.Message);
            }
        }

        public async Task LogoutAsync()
        {
            try
            {
                await _httpClient.PostAsync("auth/logout", null);
            }
            catch {  }


            SecureTokenStorage.Remove("auth_token");
            SecureTokenStorage.Remove("kullanici_rol");
            SecureTokenStorage.Remove("kullanici_adi");
            SecureTokenStorage.Remove("hasta_id");
            SecureTokenStorage.Remove("doktor_id");
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }

        public async Task<ApiResponse<bool>> RegisterAsync(RegisterRequest request)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/register", request);
                var raw = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                return new ApiResponse<bool>(raw?.Success ?? false, raw?.Message ?? "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, ex.Message);
            }
        }
        public async Task<ApiResponse<bool>> SifreDegistirAsync(SifreDegistirRequest request)
        {
            try
            {
                // Backend URL'i "change-password", query parametresi olarak kullaniciAdi ister
                var kullaniciAdi = await SecureTokenStorage.GetAsync("kullanici_adi");
                var url = !string.IsNullOrEmpty(kullaniciAdi)
                    ? $"auth/change-password?kullaniciAdi={kullaniciAdi}"
                    : "auth/change-password";

                var response = await _httpClient.PostAsJsonAsync(url, request);
                var raw = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
                return new ApiResponse<bool>(raw?.Success ?? false, raw?.Message ?? "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, ex.Message);
            }
        }
        


    }
}
