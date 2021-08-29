using System.Collections.Generic;
using System.IO;
using Chatting;
using colonyserver.Assets.UIGeneration;
using ModLoaderInterfaces;
using Newtonsoft.Json;
using Pipliz;

namespace PvP
{
    public class PvPManagement : IOnPlayerConnectedLate
    {
        public static long timeBeforeDisablingPvP = 2L * 60L * 1000L; // 2 min -> It should be configurable
        //There are specific behaviour that must be executed when enabling / disabling PvP
        private static readonly Dictionary<NetworkID, ServerTimeStamp> pvpPlayers = new Dictionary<NetworkID, ServerTimeStamp>();

        private static List<NetworkID> bannedPlayers = new List<NetworkID>();
        public static Dictionary<string, int> settings = new Dictionary<string, int>();

        /// <summary>
        /// Returns the PvP Status of a player WITHOUT considering the area in which he or the settings
        /// 
        /// For example: A player with PvP disabled in a force PvP will return false despite of being able to receive damage
        /// </summary>
        /// <param name="networkID"></param>
        /// <returns></returns>
        public static bool HasPvPEnabled(NetworkID networkID)
        {
            return pvpPlayers.ContainsKey(networkID);
        }

        /// <summary>
        /// A player CAN BE in PVP without having PvP Enabled
        /// 
        /// This happens when the player is inside of a PvP Area or the settins of PvP forces PvP to everyone
        /// </summary>
        /// <param name="networkID"></param>
        /// <returns></returns>
        public static bool IsInPvP(NetworkID networkID)
        {
            //Staff members only have PvP enable IF they enable it
            if (PermissionsManager.HasPermission(Players.GetPlayer(networkID), "khanx.pvp") && !pvpPlayers.ContainsKey(networkID))
                return false;

            if (bannedPlayers.Contains(networkID))
                return false;

            /*
             GlobalPvP: 0 = NORMAL, 1 = PvP On everyone, 2 = PvP = Off everyone
             */
            int status = settings.GetValueOrDefault("GlobalPvP", 0);

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

        public static bool EnablePvP(NetworkID networkID)
        {
            Players.Player player = Players.GetPlayer(networkID);

            if (bannedPlayers.Contains(networkID))
            {
                Chat.Send(player, "You cannot enable PvP because you are banned.");

                return false;
            }

            if (!pvpPlayers.ContainsKey(networkID))
                pvpPlayers[networkID] = ServerTimeStamp.Now;

            UIManager.AddorUpdateUILabel("PvP_On", colonyshared.NetworkUI.UIGeneration.UIElementDisplayType.Global, "PvP ON",
                                        new Vector3Int(100, -100, 100), colonyshared.NetworkUI.AnchorPresets.TopLeft,
                                        100, player, color: "#ff0000");

            Chat.Send(player, "PvP enabled.");

            return true;
        }

        public static void ResetPvPCoolDown(NetworkID networkID)
        {
            pvpPlayers[networkID] = ServerTimeStamp.Now;
        }

        public static bool DisablePvP(NetworkID networkID)
        {
            Players.Player player = Players.GetPlayer(networkID);

            if (PermissionsManager.HasPermission(Players.GetPlayer(networkID), "khanx.pvp") || bannedPlayers.Contains(networkID) || (pvpPlayers.TryGetValue(networkID, out ServerTimeStamp time) && time.TimeSinceThis > timeBeforeDisablingPvP))
            {
                pvpPlayers.Remove(networkID);

                UIManager.RemoveUILabel("PvP_On", player);

                Chat.Send(player, "PvP disabled.");

                return true;
            }

            System.TimeSpan t = System.TimeSpan.FromMilliseconds((double)(PvPManagement.timeBeforeDisablingPvP - PvPManagement.pvpPlayers[player.ID].TimeSinceThis));

            string timeToDisable;
            if (t.Minutes != 0)
                timeToDisable = string.Format("{0}m and {1}s", t.Minutes, t.Seconds);
            else
                timeToDisable = string.Format("{0:D2}s", t.Seconds);

            Chat.Send(player, string.Format("You must wait {0} before disabling PvP.", timeToDisable));

            return false;
        }

        public static void LoadBannedPlayers(List<NetworkID> bannedPlayersList)
        {
            bannedPlayers = bannedPlayersList;
        }

        public static bool IsBanned(NetworkID networkID)
        {
            return bannedPlayers.Contains(networkID);
        }

        public static List<NetworkID> GetBannedList()
        {
            return bannedPlayers;
        }

        public static void BanFromPvP(NetworkID networkID)
        {
            bannedPlayers.Add(networkID);
            DisablePvP(networkID);

            Chat.Send(Players.GetPlayer(networkID), "You have been banned from PvP");
        }

        public static void UnBanFromPvP(NetworkID networkID)
        {
            bannedPlayers.Remove(networkID);
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
