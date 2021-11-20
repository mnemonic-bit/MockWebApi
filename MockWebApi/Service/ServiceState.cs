using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MockWebApi.Service
{
    public enum ServiceState
    {
        Unknown = 0,
        NotStarted = 1,
        Running = 2,
        StopRequested = 3,
        Stopped = 4,
    }
}
