using Chatting;
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

            if(splits.Count < 2)
            {
                Chat.Send(player, "<color=red>Syntax error, use /pvp on/off </color>");
                return true;
            }

            if (splits[1].ToLower().Equals("on"))
            {
                if (!PvPManagement.pvpPlayers.ContainsKey(player.ID))
                    PvPManagement.pvpPlayers[player.ID] = ServerTimeStamp.Now;

                Chat.Send(player, "PvP enabled.");
            }
            else if (splits[1].ToLower().Equals("off"))
            {
                if (PvPManagement.CanDisablePvP(player.ID))
                    PvPManagement.pvpPlayers.Remove(player.ID);

                Chat.Send(player, "PvP disabled.");
            }
            
            return true;
        }
    }
}
