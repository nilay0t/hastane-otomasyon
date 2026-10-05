using HastaneOtomasyon.Shared.DTOs.Auth;
using HastaneOtomasyon.Shared.DTOs.Common;
namespace HastaneOtomasyon.MAUI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);
        Task<ApiResponse<bool>> RegisterAsync(RegisterRequest request);
        Task<ApiResponse<bool>> SifreDegistirAsync(SifreDegistirRequest request);
        Task LogoutAsync();
    }
}