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
            if (PermissionsManager.HasPermission(Players.GetPlayer(networkID), "khanx.pvp") && !pvpPlayers.ContainsKey(networkID) || IsBanned(networkID))
                return false;

            if (AreaManager.playersWithinAnArea.TryGetValue(networkID, out AreaType area))
            {
                return area == AreaType.PvP;
            }

            return pvpPlayers.ContainsKey(networkID);
        }

        public static bool EnablePvP(NetworkID networkID, bool verbose = true)
        {
            Players.Player player = Players.GetPlayer(networkID);

            if (IsBanned(networkID))
            {
                if(verbose)
                    Chat.Send(player, "You cannot enable PvP because you are banned.");

                return false;
            }

            if (settings.GetValueOrDefault("GlobalPvP", 0) == 2)
            {
                if (verbose)
                    Chat.Send(player, "You cannot enable PvP because it is disabled for everyone.");

                return false;
            }

            if (!pvpPlayers.ContainsKey(networkID))
                pvpPlayers[networkID] = ServerTimeStamp.Now;

            UIManager.AddorUpdateUILabel("PvP_On", colonyshared.NetworkUI.UIGeneration.UIElementDisplayType.Global, "PvP ON",
                                        new Vector3Int(100, -100, 100), colonyshared.NetworkUI.AnchorPresets.TopLeft,
                                        100, player, color: "#ff0000");
            if (verbose)
                Chat.Send(player, "PvP enabled.");

            return true;
        }

        public static void ResetPvPCoolDown(NetworkID networkID)
        {
            pvpPlayers[networkID] = ServerTimeStamp.Now;
        }

        public static bool DisablePvP(NetworkID networkID, bool forceChange = false, bool verbose = true)
        {
            Players.Player player = Players.GetPlayer(networkID);

            if (pvpPlayers.ContainsKey(networkID))
            {
                if (verbose)
                    Chat.Send(player, "PvP disabled.");

                return true;
            }

            if (PermissionsManager.HasPermission(Players.GetPlayer(networkID), "khanx.pvp") || forceChange)
            {
                pvpPlayers.Remove(networkID);

                UIManager.RemoveUILabel("PvP_On", player);

                if (verbose)
                    Chat.Send(player, "PvP disabled.");

                return true;
            }

            if(settings.GetValueOrDefault("GlobalPvP", 0) == 1)
            {
                if (verbose)
                    Chat.Send(player, "You cannot disable PvP because it is enabled for everyone.");

                return true;
            }

            if (pvpPlayers.TryGetValue(networkID, out ServerTimeStamp time) && time.TimeSinceThis > timeBeforeDisablingPvP)
            {
                pvpPlayers.Remove(networkID);

                UIManager.RemoveUILabel("PvP_On", player);

                if (verbose)
                    Chat.Send(player, "PvP disabled.");

                return true;
            }

            System.TimeSpan t = System.TimeSpan.FromMilliseconds((double)(timeBeforeDisablingPvP - pvpPlayers[player.ID].TimeSinceThis));

            string timeToDisable;
            if (t.Minutes != 0)
                timeToDisable = string.Format("{0}m and {1}s", t.Minutes, t.Seconds);
            else
                timeToDisable = string.Format("{0:D2}s", t.Seconds);

            if (verbose)
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

        public static void BanFromPvP(NetworkID networkID, bool verbose = true)
        {
            bannedPlayers.Add(networkID);
            DisablePvP(networkID, true, false);

            if(verbose)
                Chat.Send(Players.GetPlayer(networkID), "You have been banned from PvP");
        }

        public static void UnBanFromPvP(NetworkID networkID, bool verbose = true)
        {
            bannedPlayers.Remove(networkID);

            if (verbose)
                Chat.Send(Players.GetPlayer(networkID), "You have been unbanned from PvP");
        }

        public void OnPlayerConnectedLate(Players.Player player)
        {
            if (PermissionsManager.HasPermission(player, "khanx.pvp") || IsBanned(player.ID))
            {
                DisablePvP(player.ID, true, false);

                return;
            }

            int status = settings.GetValueOrDefault("GlobalPvP", 0);

            if (status == 1)
            {
                EnablePvP(player.ID);

                return;
            }
            else if(status == 2)
            {
                DisablePvP(player.ID, true);

                return;
            }

            if(pvpPlayers.TryGetValue(player.ID, out ServerTimeStamp time))
            {
                if (time.TimeSinceThis > timeBeforeDisablingPvP)
                {
                    DisablePvP(player.ID);
                }
                else
                {
                    EnablePvP(player.ID);
                }
            }
        }
    }
}
