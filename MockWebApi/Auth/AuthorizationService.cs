using MockWebApi.Configuration.Model;
using MockWebApi.Extension;
using System;
using System.Linq;

namespace MockWebApi.Auth
{
    public class AuthorizationService : IAuthorizationService
    {

        private const string BASIC_TOKEN_COOKIE_PREFIX = "Basic";
        private const string BEARER_TOKEN_COOKIE_PREFIX = "Bearer";

        private readonly IJwtService _jwtService;

        public AuthorizationService(IJwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public bool CkeckAuthorization(string authorizationHeader, EndpointDescription endpointDescription)
        {
            if (string.IsNullOrEmpty(authorizationHeader))
            {
                return false;
            }

            int splitPosition = authorizationHeader.IndexOf(' ');

            if (splitPosition == -1)
            {
                return false;
            }

            (string authorizationMethod, string authorizationHeaderValue) = authorizationHeader.SplitAt(splitPosition);

            authorizationMethod = authorizationMethod.Trim();
            authorizationHeaderValue = authorizationHeaderValue.TrimStart();

            if (string.IsNullOrEmpty(authorizationMethod))
            {
                return false;
            }

            if (string.IsNullOrEmpty(authorizationHeaderValue))
            {
                return false;
            }

            switch (authorizationMethod)
            {
                case BEARER_TOKEN_COOKIE_PREFIX:
                    return CheckJwtTokenAuthorization(authorizationHeaderValue, endpointDescription);
                case BASIC_TOKEN_COOKIE_PREFIX:
                    return CheckBasicTokenAuthorization(authorizationHeaderValue, endpointDescription);
                default:
                    throw new InvalidOperationException($"Authorization scheme is not implemented");
            }
        }

        private bool CheckJwtTokenAuthorization(string authorizationHeaderValue, EndpointDescription endpointDescription)
        {
            if (endpointDescription.AllowedUsers.IsNullOrEmpty())
            {
                return false;
            }

            JwtCredentialUser[] allowedUsers = endpointDescription
                .AllowedUsers
                .Select(user => new JwtCredentialUser()
                {
                    Name = user
                })
                .ToArray();

            if (!_jwtService.ValidateToken(authorizationHeaderValue, allowedUsers))
            {
                return false;
            }

            return true;
        }

        private bool CheckBasicTokenAuthorization(string authorizationHeaderValue, EndpointDescription endpointDescription)
        {
            //TODO: implement this check
            return true;
        }

    }
}
