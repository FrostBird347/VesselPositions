using System;
using System.Collections.Generic;
using System.Reflection;
using DarkMultiPlayerServer.Messages;
using DarkMultiPlayerCommon;

namespace VesselPositions
{
    public static class ServerTime
    {
        private static Dictionary<int, Subspace> subspaces;
        public static void Init()
        {
            //Had to use reflection as subspaces is private and we have no get method...
            subspaces = (Dictionary<int, Subspace>)typeof(WarpControl).GetField("subspaces", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        public static double GetTime()
        {
            int latestSubspaceInt = WarpControl.GetLatestSubspace();
            if (subspaces.ContainsKey(latestSubspaceInt))
            {
                Subspace latestSubspace = subspaces[latestSubspaceInt];
                double clockDiff = (DateTime.UtcNow.Ticks - latestSubspace.serverClock) / (double)TimeSpan.TicksPerSecond;
                return latestSubspace.planetTime + (clockDiff * latestSubspace.subspaceSpeed);
            }
            return 0;
        }
    }
}
