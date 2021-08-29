﻿using Chatting;
using System.Collections.Generic;
using Pipliz;

namespace PvP
{
    [ChatCommandAutoLoader]
    public class CreatePvPArea : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().Equals("/createpvparea"))
                return false;

            if (!PermissionsManager.CheckAndWarnPermission(player, "khanx.pvp.area"))
                return true;

            if (!PvPToolType.playerArea.TryGetValue(player.ID, out Area area))
            {
                Chat.Send(player, "<color=red>You have not created any area, you must use the PvP Tool to create the area.</color>");

                return true;
            }

            Vector3Int corner1 = area.min;
            Vector3Int corner2 = area.max;

            area.min = Vector3Int.Min(corner1, corner2);
            area.max = Vector3Int.Max(corner1, corner2);
            area.areaType = AreaType.PvP;

            if (!area.Contains(new Vector3Int(player.Position)))
            {
                Chat.Send(player, "<color=red>You must be inside the area to be able to create it.</ color>");

                return true;
            }

            foreach (var area2 in AreaManager.areas)
            {
                if (Area.Intersects(area, area2))
                {
                    Chat.Send(player, "<color=red>There should be no intersection of areas.</ color>");

                    return true;
                }
            }

            PvPToolType.playerArea.Remove(player.ID);
            AreaManager.areas.Add(area);
            Chat.Send(player, "<color=green>PvP Area created</color>");
            return true;
        }
    }
}
