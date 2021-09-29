using Microsoft.IdentityModel.Tokens;
using MockWebApi.Configuration.Model;

namespace MockWebApi.Auth
{
    public interface IJwtService
    {

        SigningCredentials CreateSigningCredentials(string key);

        string CreateToken(JwtCredentialUser credentialUser);

        bool ValidateToken(string token, JwtCredentialUser credentialUser);

        bool ValidateToken(string token, JwtCredentialUser[] credentialUser);
        
    }
}