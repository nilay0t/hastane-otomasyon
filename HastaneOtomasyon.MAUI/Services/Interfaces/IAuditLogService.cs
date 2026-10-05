using HastaneOtomasyon.Shared.DTOs.Common;
namespace HastaneOtomasyon.MAUI.Services.Interfaces
{
    public interface IAuditLogService
    {
        Task<ApiResponse<List<AuditLogDto>>> GetAllAsync();
       
    }
}