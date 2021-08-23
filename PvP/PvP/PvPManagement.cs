using System.Collections.Generic;
using System.IO;
using colonyserver.Assets.UIGeneration;
using ModLoaderInterfaces;
using Newtonsoft.Json;
using Pipliz;

namespace PvP
{
    public class PvPManagement : IOnPlayerConnectedLate
    {
        public static long timeBeforeDisablingPvP = 2L * 60L * 1000L; // 2 min -> It should be configurable
        public static Dictionary<NetworkID, ServerTimeStamp> pvpPlayers = new Dictionary<NetworkID, ServerTimeStamp>();

        public static List<NetworkID> bannedPlayers = new List<NetworkID>();
        public static Dictionary<string, int> settings = new Dictionary<string, int>();

        public static bool HasPvPEnabled(NetworkID networkID)
        {
            //Staff members only have PvP enable IF they enable it
            if (PermissionsManager.HasPermission(Players.GetPlayer(networkID), "khanx.pvp") && !pvpPlayers.ContainsKey(networkID))
                return false;

            if (bannedPlayers.Contains(networkID))
                return false;

            /*
             GlobalPvP: 0 = NORMAL, 1 = PvP On everyone, 2 = PvP = Off everyone
             */
            int status = PvPManagement.settings.GetValueOrDefault("GlobalPvP", 0);

            if (status == 2)
                return false;

            if (AreaManager.playersWithinAnArea.TryGetValue(networkID, out AreaType area))
            {
                return area == AreaType.PvP;
            }

            if (status == 1)
                return true;

            return pvpPlayers.ContainsKey(networkID);
        }

        public static bool CanDisablePvP(NetworkID networkID)
        {
            if (PermissionsManager.HasPermission(Players.GetPlayer(networkID), "khanx.pvp"))
                return true;

            if (pvpPlayers.TryGetValue(networkID, out ServerTimeStamp time))
            {
                return time.TimeSinceThis > timeBeforeDisablingPvP;
            }

            return true;
        }

        public void OnPlayerConnectedLate(Players.Player player)
        {
            if (pvpPlayers.ContainsKey(player.ID))
                UIManager.AddorUpdateUILabel("PvP_On", colonyshared.NetworkUI.UIGeneration.UIElementDisplayType.Global, "PvP ON",
                        new Vector3Int(100, -100, 100), colonyshared.NetworkUI.AnchorPresets.TopLeft,
                        100, player, color: "#ff0000");
        }
    }
}
