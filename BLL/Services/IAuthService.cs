using DAL.Models;

namespace BLL.Services
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(string fullName, string email, string password, string phone);
        Task<AuthResult> VerifyOtpAsync(string email, string otpCode);
        Task<AuthResult> ResendOtpAsync(string email);
        Task<AuthResult> ForgotPasswordAsync(string email);
        Task<AuthResult> VerifyResetOtpAsync(string email, string otpCode);
        Task<AuthResult> ResetPasswordAsync(string email, string token, string newPassword);
        Task<AuthResult> LoginAsync(string email, string password);
    }
}