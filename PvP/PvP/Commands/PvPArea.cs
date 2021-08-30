using Chatting;
using System.Collections.Generic;
using Pipliz;

namespace PvP
{
    [ChatCommandAutoLoader]
    public class PvPArea : IChatCommand
    {
        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (splits.Count == 0 || !splits[0].ToLower().Equals("/pvparea"))
                return false;

            if (!PermissionsManager.CheckAndWarnPermission(player, "khanx.pvp.area"))
                return true;

            if (splits.Count == 1 || splits.Count > 2)
            {
                Chat.Send(player, "<color=red>Syntax error, use /pvparea pvp/nonpvp/remove</color>");
                return true;
            }

            switch (splits[1].ToLower())
            {
                case "remove":
                    Vector3Int playerPosition = new Vector3Int(player.Position);

                    foreach (var area in AreaManager.areas)
                    {
                        if (area.Contains(playerPosition))
                        {
                            AreaManager.areas.Remove(area);
                            AreaJobTracker.SendData(player);
                            Chat.Send(player, "<color=green>Area removed.</color>");

                            return true;
                        }
                    }

                    Chat.Send(player, "<color=red>You must be inside the area to be able to remove it.</color>");
                    break;
                case "pvp":
                    TryCreateArea(player, AreaType.PvP);
                    break;
                case "nonpvp":
                    TryCreateArea(player, AreaType.NonPvP);
                    break;
                default:
                    Chat.Send(player, "<color=red>Syntax error, use /pvparea pvp/nonpvp/remove</color>");
                    break;
            }

            return true;
        }

        private static void TryCreateArea(Players.Player player, AreaType areaType)
        {
            if (!PvPToolType.playerArea.TryGetValue(player.ID, out Area area))
            {
                Chat.Send(player, "<color=red>You have not created any area, you must use the PvP Tool to create the area.</color>");

                return;
            }

            Vector3Int corner1 = area.min;
            Vector3Int corner2 = area.max;

            area.min = Vector3Int.Min(corner1, corner2);
            area.max = Vector3Int.Max(corner1, corner2);
            area.areaType = areaType;

            if (!area.Contains(new Vector3Int(player.Position)))
            {
                Chat.Send(player, "<color=red>You must be inside the area to be able to create it.</ color>");

                return;
            }

            foreach (var area2 in AreaManager.areas)
            {
                if (Area.Intersects(area, area2))
                {
                    Chat.Send(player, "<color=red>There should be no intersection of areas.</ color>");

                    return;
                }
            }

            PvPToolType.playerArea.Remove(player.ID);
            AreaManager.areas.Add(area);
            Chat.Send(player, "<color=green>" + ((areaType == AreaType.PvP) ? "PvP" : "Non PvP") + " area created</color>");
        }
    }
}
