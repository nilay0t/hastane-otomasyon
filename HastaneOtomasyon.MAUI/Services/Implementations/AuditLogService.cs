using System.Net.Http.Json;
using HastaneOtomasyon.MAUI.Services.Interfaces;
using HastaneOtomasyon.Shared.DTOs.Common;
using HastaneOtomasyon.Shared.DTOs.Auth;

namespace HastaneOtomasyon.MAUI.Services.Implementations
{
    public class AuditLogService : IAuditLogService
    {
        private readonly HttpClient _httpClient;

        public AuditLogService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ApiResponse<List<AuditLogDto>>> GetAllAsync()
        {
            try
            {
                var response = await _httpClient
                    .GetFromJsonAsync<ApiResponse<List<AuditLogDto>>>("admin/auditlogs");
                return response ?? new ApiResponse<List<AuditLogDto>>(false, "Yanıt boş");
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<AuditLogDto>>(false, ex.Message);
            }
        }
    }
}