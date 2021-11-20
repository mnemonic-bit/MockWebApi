﻿using MockWebApi.Service;
using System.Threading;

namespace MockWebApi.Extension
{
    public static class ServiceStateExtensions
    {

        public static ServiceState GetServiceState(this ThreadState threadState)
        {
            if ((threadState & ThreadState.Unstarted) > 0)
            {
                return ServiceState.NotStarted;
            }

            if ((threadState & ThreadState.Running) > 0)
            {
                return ServiceState.Running;
            }

            if ((threadState & (ThreadState.AbortRequested | ThreadState.StopRequested | ThreadState.SuspendRequested)) > 0)
            {
                return ServiceState.StopRequested;
            }

            if ((threadState & (ThreadState.Aborted | ThreadState.Stopped)) > 0)
            {
                return ServiceState.Stopped;
            }

            return ServiceState.Unknown;
        }
    }
}
