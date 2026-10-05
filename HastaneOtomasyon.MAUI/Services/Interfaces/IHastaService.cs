using HastaneOtomasyon.Shared.DTOs.Hasta;
using HastaneOtomasyon.Shared.DTOs.Common;
namespace HastaneOtomasyon.MAUI.Services.Interfaces
{
    public interface IHastaService
    {
        Task<ApiResponse<List<HastaDto>>> GetAllAsync();
        Task<ApiResponse<HastaDto>> GetByIdAsync(int id);
        Task<ApiResponse<List<HastaDto>>> AraAsync(string? ad, string? tc);
        Task<ApiResponse<bool>> CreateAsync(HastaDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int id, HastaDto dto);
    }
}