using System;

namespace MockWebApi.Configuration.Model
{
    public class JwtServiceOptions
    {

        public string Issuer { get; set; }

        public string Audience { get; set; }

        public TimeSpan Expiration { get; set; }

        public string SigningKey { get; set; }

    }
}
