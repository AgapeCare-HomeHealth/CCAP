using CCAP.Data.DTOs.Authentication;


namespace CCAP.Data.Interfaces
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}
