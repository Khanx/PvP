using Chatting;
using colonyserver.Assets.UIGeneration;
using Pipliz;
using System;
using System.Collections.Generic;

namespace PvP
{
    [ChatCommandAutoLoader]
    public class PvPOnOff : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (splits.Count == 0 || !splits[0].ToLower().Equals("/pvp"))
                return false;

            if (splits.Count <= 1 || splits.Count > 2)
            {
                Chat.Send(player, "<color=red>Syntax error, use /pvp on/off </color>");
                return true;
            }

            if(PvPManagement.IsBanned(player.ID))
            {
                Chat.Send(player, "You are banned from PvP.");

                return true;
            }

            if (splits[1].ToLower().Equals("on"))
            {
                PvPManagement.EnablePvP(player.ID);
            }
            else if (splits[1].ToLower().Equals("off"))
            {
                PvPManagement.DisablePvP(player.ID);
            }

            return true;
        }
    }
}
