using System;
using System.Diagnostics.CodeAnalysis;
using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public class EndpointState : IEndpointState
    {

        public EndpointDescription EndpointDescription
        {
            get
            {
                return _endpointDescription;
            }
        }

        public EndpointState(EndpointDescription endpointDescription)
        {
            if (endpointDescription == null)
            {
                throw new ArgumentNullException(nameof(endpointDescription));
            }

            _endpointDescription = endpointDescription;
            _currentPos = 0;
        }

        public bool HasNext()
        {
            return _endpointDescription.Results != null && _currentPos < _endpointDescription.Results.Length;
        }

        public void Reset()
        {
            _currentPos = 0;
        }

        public bool TryGetHttpResult([NotNullWhen(true)] out HttpResult? httpResult)
        {
            httpResult = default;

            if (_endpointDescription.Results != null && _currentPos < _endpointDescription.Results.Length)
            {
                httpResult = _endpointDescription.Results[_currentPos++];
                return true;
            }

            return false;
        }

        private EndpointDescription _endpointDescription;
        private int _currentPos;

    }
}
