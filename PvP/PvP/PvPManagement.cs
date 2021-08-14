using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvP
{
    public class PvPManagement
    {
        public static long timeBeforeDisablingPvP = 2L * 60L * 1000L; // 2 min -> It should be configurable
        public static Dictionary<NetworkID, ServerTimeStamp> pvpPlayers = new Dictionary<NetworkID, ServerTimeStamp>();

        public static bool HasPvPEnabled(NetworkID networkID)
        {
            //ADD STAFF HERE

            if (AreaManager.playersWithinAnArea.TryGetValue(networkID, out AreaType area))
            {
                return area == AreaType.PvP;
            }

            return pvpPlayers.ContainsKey(networkID);
        }

        public static bool CanDisablePvP(NetworkID networkID)
        {
            if (pvpPlayers.TryGetValue(networkID, out ServerTimeStamp time))
            {
                return time.TimeSinceThis < timeBeforeDisablingPvP;
            }

            return true;
        }
    }
}
