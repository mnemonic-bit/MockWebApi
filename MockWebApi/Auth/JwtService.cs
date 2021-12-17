using System;
using System.Linq;
using System.Security.Claims;
using System.Text;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

using MockWebApi.Configuration;
using MockWebApi.Configuration.Model;

using Newtonsoft.Json;

using JwtSecurityToken = System.IdentityModel.Tokens.Jwt.JwtSecurityToken;
using JwtSecurityTokenHandler = System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler;

namespace MockWebApi.Auth
{
    public class JwtService : IJwtService
    {

        private readonly JwtServiceOptions _options;

        private readonly SigningCredentials _signingCredentials;
        private readonly JwtSecurityTokenHandler _jwtSecurityTokenHandler;

        public JwtService(IServiceConfiguration serviceConfiguration)
        {
            _options = serviceConfiguration.JwtServiceOptions;
            _signingCredentials = CreateSigningCredentials(_options.SigningKey);
            _jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        }

        public SigningCredentials CreateSigningCredentials(string key)
        {
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            return signingCredentials;
        }

        public string CreateToken(JwtCredentialUser credentialUser)
        {
            if (credentialUser == null || credentialUser.Name == null)
            {
                return string.Empty;
            }

            DateTime now = DateTime.UtcNow;

            Claim[] claims = new Claim[]
            {
                new Claim(ClaimTypes.Name, credentialUser.Name),
                new Claim(JwtRegisteredClaimNames.Sub, JsonConvert.SerializeObject(credentialUser)),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, now.ToString(), ClaimValueTypes.Integer64),
                new Claim(ClaimTypes.CookiePath, "Authorization"),
                new Claim(ClaimTypes.Role, "ROLE")
            };

            JwtSecurityToken jwt = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(_options.Expiration),
                signingCredentials: _signingCredentials);

            string encodedJwt = _jwtSecurityTokenHandler.WriteToken(jwt);

            return encodedJwt;
        }

        public bool ValidateToken(string token, JwtCredentialUser credentialUser)
        {
            return ValidateToken(token, new JwtCredentialUser[] { credentialUser });
        }

        public bool ValidateToken(string token, JwtCredentialUser[] allowedUsers)
        {
            if (!_jwtSecurityTokenHandler.CanReadToken(token))
            {
                return false;
            }

            TokenValidationParameters validationParameters = new TokenValidationParameters
            {
                ValidIssuer = _options.Issuer,
                ValidateIssuer = true,
                ValidAudience = _options.Audience,
                ValidateAudience = true,
                IssuerSigningKey = _signingCredentials.Key,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            if (!TryValidateToken(token, validationParameters, out ClaimsPrincipal claimsPrincipal, out SecurityToken securityToken))
            {
                return false;
            }

            return allowedUsers.Where(user => user.Name.Equals(claimsPrincipal.Identity.Name)).Any();

            // Examples of how to use the ClaimsPrincipal:
            //bool hasEmailClaim = claimsPrincipal.HasClaim(c => c.Type == ClaimTypes.Email);
            //var emailClaim = claimsPrincipal.Claims.Where(c => c.Type == ClaimTypes.Email).First().Value;
        }

        private bool TryValidateToken(string token, TokenValidationParameters validationParameters, out ClaimsPrincipal claimsPrincipal, out SecurityToken validatedToken)
        {
            claimsPrincipal = default;
            validatedToken = default;

            try
            {
                claimsPrincipal = _jwtSecurityTokenHandler.ValidateToken(token, validationParameters, out validatedToken);
                return true;
            }
            catch
            {
            }

            return false;
        }

    }
}
