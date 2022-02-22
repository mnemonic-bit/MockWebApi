using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace MockWebApi.Model
{
    public class ExtendedResponseCookies : IResponseCookies
    {

        private readonly Dictionary<string, string> _cookieJar;

        public ExtendedResponseCookies()
        {
            _cookieJar = new Dictionary<string, string>();
        }

        public void Append(string key, string value)
        {
            _cookieJar[key] = value;
        }

        public void Append(string key, string value, CookieOptions options)
        {
            throw new NotImplementedException();
        }

        public void Delete(string key)
        {
            _cookieJar.Remove(key);
        }

        public void Delete(string key, CookieOptions options)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out string? value)
        {
            return _cookieJar.TryGetValue(key, out value);
        }

    }
}
