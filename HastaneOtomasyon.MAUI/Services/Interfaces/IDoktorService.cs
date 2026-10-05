using HastaneOtomasyon.Shared.DTOs.Doktor;
using HastaneOtomasyon.Shared.DTOs.Common;
namespace HastaneOtomasyon.MAUI.Services.Interfaces
{
    public interface IDoktorService
    {
        Task<ApiResponse<List<DoktorDto>>> GetAllAsync();
        Task<ApiResponse<DoktorDto>> GetByIdAsync(int id);
        Task<ApiResponse<List<DoktorDto>>> GetByPoliklinikAsync(int poliklinikId);
    }
}