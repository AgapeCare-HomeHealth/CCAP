using CCAP.Data.Entities;


namespace CCAP.Data.Interfaces
{
    public interface IJwtService
    {
        string GenerateToken(ApplicationUser user);
    }
}
