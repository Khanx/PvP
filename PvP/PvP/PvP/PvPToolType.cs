using Pipliz;
using System.Collections.Generic;
using ModLoaderInterfaces;
using static Shared.PlayerClickedData;

namespace PvP
{
    public class PvPToolType : IAfterWorldLoad, IOnPlayerClicked, IOnSendAreaHighlights
    {
        public static Dictionary<Players.PlayerIDShort, Area> playerArea = new Dictionary<Players.PlayerIDShort, Area>();

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

            if (!PermissionsManager.CheckAndWarnPermission(player, "khanx.pvp.area"))
                return;

            if (playerClickedData.ClickType == EClickType.Left)
            {
                var area = playerArea.GetValueOrDefault(player.ID.ID, new Area(playerClickedData.GetVoxelHit().BlockHit + Vector3Int.up, playerClickedData.GetVoxelHit().BlockHit + Vector3Int.up, AreaType.NotDefined));
                area.min = playerClickedData.GetVoxelHit().BlockHit + Vector3Int.up;
                playerArea[player.ID.ID] = area;

                AreaJobTracker.SendData(player);
            }
            else if (playerClickedData.ClickType == EClickType.Right)
            {
                var area = playerArea.GetValueOrDefault(player.ID.ID, new Area(playerClickedData.GetVoxelHit().BlockHit + Vector3Int.up, playerClickedData.GetVoxelHit().BlockHit + Vector3Int.up, AreaType.NotDefined));
                area.max = playerClickedData.GetVoxelHit().BlockHit + Vector3Int.up;
                playerArea[player.ID.ID] = area;

                AreaJobTracker.SendData(player);
            }
        }

        public void OnSendAreaHighlights(Players.Player player, List<AreaJobTracker.AreaHighlight> list, List<ushort> showWhileHoldingTypes)
        {
            if (null == player || !PermissionsManager.HasPermission(player, "khanx.pvp.area"))
                return;

            showWhileHoldingTypes.Add(pvpTool);

            foreach (var area in AreaManager.areas)
            {
                AreaJobTracker.AreaHighlight newArea = new AreaJobTracker.AreaHighlight();
                newArea.Minimum = area.min;
                newArea.Maximum = area.max;
                newArea.MeshType = Shared.EAreaMeshType.AutoSelect;
                newArea.AreaType = Shared.EServerAreaType.ConstructionArea;

                list.Add(newArea);
            }

            var areaP = playerArea.GetValueOrDefault(player.ID.ID, new Area(Vector3Int.invalidPos, Vector3Int.invalidPos, AreaType.NotDefined));

            AreaJobTracker.AreaHighlight newArea2 = new AreaJobTracker.AreaHighlight();
            newArea2.Minimum = Vector3Int.Min(areaP.min, areaP.max);
            newArea2.Maximum = Vector3Int.Max(areaP.min, areaP.max);
            newArea2.MeshType = Shared.EAreaMeshType.AutoSelect;
            newArea2.AreaType = Shared.EServerAreaType.ConstructionArea;

            list.Add(newArea2);
        }
    }
}
