using BLL.Services;
using DAL.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace BLL.Services.Implementation
{
    public class AuthService : IAuthService
    {
        private readonly IUserService _userService;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailService _emailService;

        public AuthService(IUserService userService, IMemoryCache memoryCache, IEmailService emailService)
        {
            _userService = userService;
            _memoryCache = memoryCache;
            _emailService = emailService;
        }

        public async Task<AuthResult> RegisterAsync(string fullName, string email, string password, string phone)
        {
            var existingUser = await _userService.GetUserByEmail(email);
            if (existingUser != null && existingUser.IsEmailVerified)
            {
                return new AuthResult { Success = false, Message = "Email is already taken." };
            }

            var otp = new Random().Next(100000, 999999).ToString();
            _memoryCache.Set($"OTP_{email}", otp, TimeSpan.FromMinutes(5));

            if (existingUser == null)
            {
                await _userService.Register(fullName, email, password, phone);
            }
            else
            {
                existingUser.DisplayName = fullName;
                existingUser.PasswordHash = _userService.HashPassword(password);
                existingUser.Phone = phone;
                existingUser.UpdatedAt = DateTime.UtcNow;
                await _userService.UpdateUser(existingUser);
            }

            try
            {
                await _emailService.SendEmailAsync(email, "Estately - Verify your email", 
                    $"<h3>Welcome to Estately!</h3><p>Your verification code is: <strong>{otp}</strong></p><p>This code expires in 5 minutes.</p>");
            }
            catch
            {
                var userToDelete = await _userService.GetUserByEmail(email);
                if (userToDelete != null && !userToDelete.IsEmailVerified)
                {
                    await _userService.DeleteUser(userToDelete);
                }
                return new AuthResult { Success = false, Message = "Failed to send verification email." };
            }

            return new AuthResult { Success = true, Message = "Registration successful. Please verify your email.", Email = email };
        }

        public async Task<AuthResult> VerifyOtpAsync(string email, string otpCode)
        {
            if (!_memoryCache.TryGetValue($"OTP_{email}", out string? storedOtp) || string.IsNullOrWhiteSpace(storedOtp))
            {
                return new AuthResult { Success = false, Message = "OTP has expired or is invalid." };
            }

            if (storedOtp != otpCode)
            {
                return new AuthResult { Success = false, Message = "Invalid OTP code." };
            }

            var user = await _userService.GetUserByEmail(email);
            if (user != null)
            {
                user.IsEmailVerified = true;
                user.IsActive = true;
                await _userService.UpdateUser(user);
                _memoryCache.Remove($"OTP_{email}");
                return new AuthResult { Success = true, Message = "Email verified successfully." };
            }

            return new AuthResult { Success = false, Message = "User not found." };
        }

        public async Task<AuthResult> ResendOtpAsync(string email)
        {
            var user = await _userService.GetUserByEmail(email);
            if (user == null) return new AuthResult { Success = false, Message = "User not found." };
            if (user.IsEmailVerified) return new AuthResult { Success = false, Message = "Email is already verified." };

            if (_memoryCache.TryGetValue($"OTP_Cooldown_{email}", out _))
            {
                return new AuthResult { Success = false, Message = "Please wait 60 seconds before requesting a new code." };
            }

            var otp = new Random().Next(100000, 999999).ToString();
            _memoryCache.Set($"OTP_{email}", otp, TimeSpan.FromMinutes(5));
            _memoryCache.Set($"OTP_Cooldown_{email}", true, TimeSpan.FromSeconds(60));

            try
            {
                await _emailService.SendEmailAsync(email, "Estately - Verify your email", 
                    $"<h3>Verification Code</h3><p>Your verification code is: <strong>{otp}</strong></p><p>This code expires in 5 minutes.</p>");
            }
            catch
            {
                return new AuthResult { Success = false, Message = "Failed to send email." };
            }

            return new AuthResult { Success = true, Message = "OTP resent successfully." };
        }

        public async Task<AuthResult> ForgotPasswordAsync(string email)
        {
            var user = await _userService.GetUserByEmail(email);
            if (user == null) return new AuthResult { Success = false, Message = "Email address not found." };

            if (_memoryCache.TryGetValue($"RESET_OTP_Cooldown_{email}", out _))
            {
                return new AuthResult { Success = false, Message = "Please wait 60 seconds before requesting a new code." };
            }

            var otp = new Random().Next(100000, 999999).ToString();
            _memoryCache.Set($"RESET_OTP_{email}", otp, TimeSpan.FromMinutes(5));
            _memoryCache.Set($"RESET_OTP_Cooldown_{email}", true, TimeSpan.FromSeconds(60));

            try
            {
                await _emailService.SendEmailAsync(email, "Estately - Reset Password OTP", 
                    $"<h3>Reset Password</h3><p>Your OTP code is: <strong>{otp}</strong></p><p>This code expires in 5 minutes.</p>");
            }
            catch
            {
                return new AuthResult { Success = false, Message = "Failed to send email." };
            }

            return new AuthResult { Success = true, Message = "OTP sent successfully." };
        }

        public Task<AuthResult> VerifyResetOtpAsync(string email, string otpCode)
        {
            if (!_memoryCache.TryGetValue($"RESET_OTP_{email}", out string? storedOtp) || string.IsNullOrWhiteSpace(storedOtp))
            {
                return Task.FromResult(new AuthResult { Success = false, Message = "OTP has expired or is invalid." });
            }

            if (storedOtp != otpCode)
            {
                return Task.FromResult(new AuthResult { Success = false, Message = "Invalid OTP code." });
            }

            var resetToken = Guid.NewGuid().ToString();
            _memoryCache.Set($"RESET_TOKEN_{email}", resetToken, TimeSpan.FromMinutes(10));
            _memoryCache.Remove($"RESET_OTP_{email}");

            return Task.FromResult(new AuthResult { Success = true, Message = "OTP verified.", Token = resetToken });
        }

        public async Task<AuthResult> ResetPasswordAsync(string email, string token, string newPassword)
        {
            if (!_memoryCache.TryGetValue($"RESET_TOKEN_{email}", out string? storedToken) || string.IsNullOrWhiteSpace(storedToken))
            {
                return new AuthResult { Success = false, Message = "Reset session expired. Please start over." };
            }

            if (storedToken != token)
            {
                return new AuthResult { Success = false, Message = "Invalid token." };
            }

            return await ResetPasswordInternalAsync(email, newPassword);
        }

        private async Task<AuthResult> ResetPasswordInternalAsync(string email, string newPassword)
        {
            var user = await _userService.GetUserByEmail(email);
            if (user == null) return new AuthResult { Success = false, Message = "User not found." };

            if (!string.IsNullOrEmpty(user.PasswordHash) && _userService.VerifyPassword(newPassword, user.PasswordHash))
            {
                return new AuthResult { Success = false, Message = "You have entered your old password. Please choose a different password." };
            }

            user.PasswordHash = _userService.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userService.UpdateUser(user);
            _memoryCache.Remove($"RESET_TOKEN_{email}");

            return new AuthResult { Success = true, Message = "Password reset successfully." };
        }

        public async Task<AuthResult> LoginAsync(string email, string password)
        {
            var user = await _userService.Login(email, password);
            if (user == null)
            {
                return new AuthResult { Success = false, Message = "Invalid email or password." };
            }

            if (!user.IsEmailVerified)
            {
                return new AuthResult { Success = false, Message = "Please verify your email address before logging in." };
            }

            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user.Email}:{user.Id}:{DateTime.Now.Ticks}"));
            return new AuthResult { Success = true, Token = token, Message = "Login successful.", Email = user.DisplayName }; // Returning DisplayName in Email prop for now or add Name prop
        }
    }
}