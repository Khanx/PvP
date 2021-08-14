using Pipliz;
using System.Collections.Generic;
using ModLoaderInterfaces;
using static Shared.PlayerClickedData;

namespace PvP
{
    public class PvPToolType : IAfterWorldLoad, IOnPlayerClicked, IOnSendAreaHighlights
    {
        public static Dictionary<NetworkID, Area> playerArea = new Dictionary<NetworkID, Area>();

        public static ushort pvpTool = 0;

        public void AfterWorldLoad()
        {
            if (!ItemTypes.IndexLookup.TryGetIndex("Khanx.PvPTool", out pvpTool))
                pvpTool = ushort.MaxValue;
        }

        public void OnPlayerClicked(Players.Player player, Shared.PlayerClickedData playerClickedData)
        {
            if (playerClickedData.TypeSelected != pvpTool)
                return;

            if (playerClickedData.HitType != EHitType.Block || playerClickedData.GetVoxelHit().TypeHit == BlockTypes.BuiltinBlocks.Indices.air)
                return;

            if (!PermissionsManager.CheckAndWarnPermission(player, "khanx.pvp"))
                return;

            if (playerClickedData.ClickType == EClickType.Left)
            {
                var area = playerArea.GetValueOrDefault(player.ID, new Area(playerClickedData.GetVoxelHit().BlockHit + Vector3Int.up, playerClickedData.GetVoxelHit().BlockHit + Vector3Int.up, AreaType.NotDefined));
                area.min = playerClickedData.GetVoxelHit().BlockHit + Vector3Int.up;
                playerArea[player.ID] = area;

                AreaJobTracker.SendData(player);
            }
            else if (playerClickedData.ClickType == EClickType.Right)
            {
                var area = playerArea.GetValueOrDefault(player.ID, new Area(playerClickedData.GetVoxelHit().BlockHit + Vector3Int.up, playerClickedData.GetVoxelHit().BlockHit + Vector3Int.up, AreaType.NotDefined));
                area.max = playerClickedData.GetVoxelHit().BlockHit + Vector3Int.up;
                playerArea[player.ID] = area;

                AreaJobTracker.SendData(player);
            }
        }

        public void OnSendAreaHighlights(Players.Player player, List<AreaJobTracker.AreaHighlight> list, List<ushort> showWhileHoldingTypes)
        {
            if (null == player || !PermissionsManager.HasPermission(player, "khanx.pvp"))
                return;

            showWhileHoldingTypes.Add(pvpTool);

            foreach (var area in AreaManager.areas)
            {
                list.Add(new AreaJobTracker.AreaHighlight(area.min, area.max, Shared.EAreaMeshType.AutoSelect, Shared.EServerAreaType.ConstructionArea));
            }

            var areaP = playerArea.GetValueOrDefault(player.ID, new Area(Vector3Int.invalidPos, Vector3Int.invalidPos, AreaType.NotDefined));

            list.Add(new AreaJobTracker.AreaHighlight(Vector3Int.Min(areaP.min, areaP.max), Vector3Int.Max(areaP.min, areaP.max), Shared.EAreaMeshType.AutoSelect, Shared.EServerAreaType.ConstructionArea));
        }
    }
}
