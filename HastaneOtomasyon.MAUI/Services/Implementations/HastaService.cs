using System.Net.Http.Json;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Hasta;

namespace HastaneOtomasyon.MAUI.Services.Implementations
{
    public class HastaService : IHastaService
    {
        private readonly HttpClient _httpClient;

        public HastaService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<List<HastaDto>>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<List<HastaDto>>>("hastalar");
                return response ?? new ApiResponse<List<HastaDto>>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<HastaDto>>(false, ex.Message);
            }
        }

        public async Task<ApiResponse<HastaDto>> GetByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<HastaDto>>($"hastalar/{id}");
                return response ?? new ApiResponse<HastaDto>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<HastaDto>(false, ex.Message);
            }
        }

        public async Task<ApiResponse<List<HastaDto>>> AraAsync(string? ad, string? tc)
        {
            try
            {
                var url = $"hastalar/ara?ad={ad}&tc={tc}";
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<List<HastaDto>>>(url);
                return response ?? new ApiResponse<List<HastaDto>>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<HastaDto>>(false, ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> CreateAsync(HastaDto dto)
        {
            try
            {
                var msg = await _httpClient.PostAsJsonAsync("hastalar", dto);
                var result = await msg.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result ?? new ApiResponse<bool>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, ex.Message);
            }
        }

        public async Task<ApiResponse<bool>> UpdateAsync(int id, HastaDto dto)
        {
            try
            {
                var msg = await _httpClient.PutAsJsonAsync($"hastalar/{id}", dto);
                var result = await msg.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                return result ?? new ApiResponse<bool>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<bool>(false, ex.Message);
            }
        }
    }
}