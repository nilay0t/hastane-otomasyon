using HastaneOtomasyon.Shared.DTOs.Randevu;
using HastaneOtomasyon.Shared.DTOs.Common;
namespace HastaneOtomasyon.MAUI.Services.Interfaces
{
    public interface IRandevuService
    {
        Task<ApiResponse<List<RandevuListDto>>> GetAllAsync();
        Task<ApiResponse<RandevuListDto>> GetByIdAsync(int id);
        Task<ApiResponse<List<RandevuListDto>>> GetByHastaIdAsync(int hastaId);
        Task<ApiResponse<List<RandevuListDto>>> GetByDoktorIdAsync(int doktorId);
        Task<ApiResponse<List<RandevuListDto>>> GetByTarihAsync(DateTime bas, DateTime bit);
        Task<ApiResponse<bool>> CreateAsync(RandevuCreateDto dto);
        Task<ApiResponse<bool>> UpdateAsync(int id, RandevuUpdateDto dto);
        Task<ApiResponse<bool>> DeleteAsync(int id);
    }
}