
using Microsoft.AspNetCore.Identity;

namespace TemplateAPIProject.Repositories
{
    public interface ITokenRepository
    {
        string CreateJWTToken(IdentityUser user, List<string> roles);
    }
}
