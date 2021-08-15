using Chatting;
using colonyserver.Assets.UIGeneration;
using Pipliz;
using System;
using System.Collections.Generic;

namespace PvP.Commands
{
    [ChatCommandAutoLoader]
    public class PvPOnOff : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (splits.Count == 0 || !splits[0].ToLower().Equals("/pvp"))
                return false;

            if (splits.Count < 2)
            {
                Chat.Send(player, "<color=red>Syntax error, use /pvp on/off </color>");
                return true;
            }

            if (splits[1].ToLower().Equals("on"))
            {
                if (!PvPManagement.pvpPlayers.ContainsKey(player.ID))
                    PvPManagement.pvpPlayers[player.ID] = ServerTimeStamp.Now;

                UIManager.AddorUpdateUILabel("PvP_On", colonyshared.NetworkUI.UIGeneration.UIElementDisplayType.Global, "PvP ON",
                    new Vector3Int(100, -100, 100), colonyshared.NetworkUI.AnchorPresets.TopLeft,
                    100, player, color: "#ff0000");

                Chat.Send(player, "PvP enabled.");
            }
            else if (splits[1].ToLower().Equals("off"))
            {
                if (PvPManagement.CanDisablePvP(player.ID))
                {
                    PvPManagement.pvpPlayers.Remove(player.ID);
                    UIManager.RemoveUILabel("PvP_On", player);

                    Chat.Send(player, "PvP disabled.");
                }
                else
                {

                    TimeSpan t = TimeSpan.FromMilliseconds((double) (PvPManagement.timeBeforeDisablingPvP - PvPManagement.pvpPlayers[player.ID].TimeSinceThis));
                    string time;
                    if(t.Minutes != 0)
                        time = string.Format("{0}m and {1}s", t.Minutes, t.Seconds);
                    else
                        time = string.Format("{0:D2}s", t.Seconds);

                    Chat.Send(player, string.Format("You must wait {0} before disabling PvP.", time));
                }
            }

            return true;
        }
    }
}
