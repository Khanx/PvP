using Chatting;
using System.Collections.Generic;
using Pipliz;

namespace PvP.Commands
{
    [ChatCommandAutoLoader]
    public class RemovePvPArea : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().Equals("/removenonpvparea") && !chat.Trim().ToLower().Equals("/removepvparea"))
                return false;

            if (!PermissionsManager.CheckAndWarnPermission(player, "khanx.pvp"))
                return true;

            Vector3Int playerPosition = new Vector3Int(player.Position);

            foreach(var area in AreaManager.areas)
            {
                if(area.Contains(playerPosition))
                {
                    Chat.Send(player, "<color=green>Area removed.</color>");
                    AreaManager.areas.Remove(area);
                    AreaJobTracker.SendData(player);

                    return true;
                }
            }

            Chat.Send(player, "<color=red>You must be inside the area to be able to remove it.</color>");

            return true;
        }
    }
}
