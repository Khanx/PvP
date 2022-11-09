using System.Collections.Generic;
using Chatting;
using colonyserver.Assets.UIGeneration;
using ModLoaderInterfaces;
using Pipliz;

namespace PvP
{
    public class PvPManagement : IOnPlayerConnectedLate
    {
        public static long timeBeforeDisablingPvP = 2L * 60L * 1000L; // 2 min -> It should be configurable
        //There are specific behaviour that must be executed when enabling / disabling PvP
        private static readonly Dictionary<Players.PlayerIDShort, ServerTimeStamp> pvpPlayers = new Dictionary<Players.PlayerIDShort, ServerTimeStamp>();

        private static List<Players.PlayerIDShort> bannedPlayers = new List<Players.PlayerIDShort>();
        public static Dictionary<string, int> settings = new Dictionary<string, int>();

        /// <summary>
        /// Returns the PvP Status of a player WITHOUT considering the area in which he or the settings
        /// 
        /// For example: A player with PvP disabled in a force PvP will return false despite of being able to receive damage
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public static bool HasPvPEnabled(Players.PlayerIDShort playerID)
        {
            return pvpPlayers.ContainsKey(playerID);
        }

        /// <summary>
        /// A player CAN BE in PVP without having PvP Enabled
        /// 
        /// This happens when the player is inside of a PvP Area or the settins of PvP forces PvP to everyone
        /// </summary>
        /// <param name="playerID"></param>
        /// <returns></returns>
        public static bool IsInPvP(Players.PlayerIDShort playerID)
        {
            //Staff members only have PvP enable IF they enable it
            if (PermissionsManager.HasPermission(Extender.GetPlayer(playerID), "khanx.pvp") && !pvpPlayers.ContainsKey(playerID) || IsBanned(playerID))
                return false;

            if (AreaManager.playersWithinAnArea.TryGetValue(playerID, out AreaType area))
            {
                return area == AreaType.PvP;
            }

            return pvpPlayers.ContainsKey(playerID);
        }

        public static bool EnablePvP(Players.PlayerIDShort playerID, bool verbose = true)
        {
            Players.Player player = Extender.GetPlayer(playerID);

            if (IsBanned(playerID))
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

            if (!pvpPlayers.ContainsKey(playerID))
                pvpPlayers[playerID] = ServerTimeStamp.Now;

            UIManager.AddorUpdateUILabel("PvP_On", colonyshared.NetworkUI.UIGeneration.UIElementDisplayType.Global, "PvP ON",
                                        new Vector3Int(100, -100, 100), colonyshared.NetworkUI.AnchorPresets.TopLeft,
                                        100, player, color: "#ff0000");
            if (verbose)
                Chat.Send(player, "PvP enabled.");

            PvPPlayerSkin.ChangePlayerSkin(playerID);

            return true;
        }

        public static void ResetPvPCoolDown(Players.PlayerIDShort playerID)
        {
            pvpPlayers[playerID] = ServerTimeStamp.Now;
        }

        public static bool DisablePvP(Players.PlayerIDShort playerID, bool forceChange = false, bool verbose = true)
        {
            Players.Player player = Extender.GetPlayer(playerID);

            if (!pvpPlayers.ContainsKey(playerID))
            {
                if (verbose)
                    Chat.Send(player, "PvP disabled.");

                return true;
            }

            if (PermissionsManager.HasPermission(Extender.GetPlayer(playerID), "khanx.pvp") || forceChange)
            {
                pvpPlayers.Remove(playerID);

                UIManager.RemoveUILabel("PvP_On", player);
                PvPPlayerSkin.ChangePlayerSkin(playerID);

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

            if (pvpPlayers.TryGetValue(playerID, out ServerTimeStamp time) && time.TimeSinceThis > timeBeforeDisablingPvP)
            {
                pvpPlayers.Remove(playerID);

                UIManager.RemoveUILabel("PvP_On", player);
                PvPPlayerSkin.ChangePlayerSkin(playerID);

                if (verbose)
                    Chat.Send(player, "PvP disabled.");

                return true;
            }

            System.TimeSpan t = System.TimeSpan.FromMilliseconds((double)(timeBeforeDisablingPvP - pvpPlayers[player.ID.ID].TimeSinceThis));

            string timeToDisable;
            if (t.Minutes != 0)
                timeToDisable = string.Format("{0}m and {1}s", t.Minutes, t.Seconds);
            else
                timeToDisable = string.Format("{0:D2}s", t.Seconds);

            if (verbose)
                Chat.Send(player, string.Format("You must wait {0} before disabling PvP.", timeToDisable));

            return false;
        }

        public static void LoadBannedPlayers(List<Players.PlayerIDShort> bannedPlayersList)
        {
            bannedPlayers = bannedPlayersList;
        }

        public static bool IsBanned(Players.PlayerIDShort playerID)
        {
            return bannedPlayers.Contains(playerID);
        }

        public static List<Players.PlayerIDShort> GetBannedList()
        {
            return bannedPlayers;
        }

        public static void BanFromPvP(Players.PlayerIDShort playerID, bool verbose = true)
        {
            bannedPlayers.Add(playerID);
            DisablePvP(playerID, true, false);

            if(verbose)
                Chat.Send(Extender.GetPlayer(playerID), "You have been banned from PvP");
        }

        public static void UnBanFromPvP(Players.PlayerIDShort playerID, bool verbose = true)
        {
            bannedPlayers.Remove(playerID);

            if (verbose)
                Chat.Send(Extender.GetPlayer(playerID), "You have been unbanned from PvP");
        }

        public void OnPlayerConnectedLate(Players.Player player)
        {
            if (PermissionsManager.HasPermission(player, "khanx.pvp") || IsBanned(player.ID.ID))
            {
                DisablePvP(player.ID.ID, true, false);

                return;
            }

            int status = settings.GetValueOrDefault("GlobalPvP", 0);

            if (status == 1)
            {
                EnablePvP(player.ID.ID);

                return;
            }
            else if(status == 2)
            {
                DisablePvP(player.ID.ID, true);

                return;
            }

            if(pvpPlayers.TryGetValue(player.ID.ID, out ServerTimeStamp time))
            {
                if (time.TimeSinceThis > timeBeforeDisablingPvP)
                {
                    DisablePvP(player.ID.ID);
                }
                else
                {
                    EnablePvP(player.ID.ID);
                }
            }
        }
    }
}
