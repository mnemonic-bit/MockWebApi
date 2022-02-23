using System.Diagnostics.CodeAnalysis;
using MockWebApi.Configuration.Model;

namespace MockWebApi.Configuration
{
    public interface IEndpointState
    {
        EndpointDescription EndpointDescription { get; }

        bool HasNext();
        void Reset();
        bool TryGetHttpResult([NotNullWhen(true)] out HttpResult? httpResult);
    }
}