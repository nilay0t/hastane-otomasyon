using System.Net.Http.Json;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Doktor;

namespace HastaneOtomasyon.MAUI.Services.Implementations
{
    public class DoktorService : IDoktorService
    {
        private readonly HttpClient _httpClient;

        public DoktorService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<List<DoktorDto>>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<List<DoktorDto>>>("doktorlar");
                return response ?? new ApiResponse<List<DoktorDto>>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<DoktorDto>>(false, ex.Message);
            }
        }

        public async Task<ApiResponse<DoktorDto>> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<DoktorDto>>($"doktorlar/{id}");
                return response ?? new ApiResponse<DoktorDto>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<DoktorDto>(false, ex.Message);
            }
        }

        public async Task<ApiResponse<List<DoktorDto>>> GetByPoliklinikAsync(int poliklinikId)
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<List<DoktorDto>>>(
                        $"doktorlar?poliklinikId={poliklinikId}");
                return response ?? new ApiResponse<List<DoktorDto>>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<DoktorDto>>(false, ex.Message);
            }
        }
    }
}