using Pipliz;
using System.Collections.Generic;
using ModLoaderInterfaces;
using Newtonsoft.Json;
using System.IO;

namespace PvP
{
    public enum AreaType
    {
        NotDefined,
        PvP,
        NonPvP
    }

    public struct Area
    {
        public Vector3Int min;
        public Vector3Int max;
        public AreaType areaType;

        public Area(Vector3Int corner1, Vector3Int corner2, AreaType areaType)
        {
            this.min = Vector3Int.Min(corner1, corner2);
            this.max = Vector3Int.Max(corner1, corner2);
            this.areaType = areaType;
        }

        //From Pipliz.BoundsInt
        public bool Contains(Vector3Int v)
        {
            return v >= min && v <= max;
        }

        //From Pipliz.BoundsInt
        public static bool Intersects(Area areaA, Area areaB)
        {
            return areaA.max.x >= areaB.min.x && areaA.max.y >= areaB.min.y && areaA.max.z >= areaB.min.z && areaA.min.x <= areaB.max.x && areaA.min.y <= areaB.max.y && areaA.min.z <= areaB.max.z;
        }
    }

    public class AreaManager : IAfterWorldLoad, IOnAutoSaveWorld, IOnQuit, IOnPlayerMoved
    {
        public static List<Area> areas = new List<Area>();
        public static Dictionary<NetworkID, AreaType> playersWithinAnArea = new Dictionary<NetworkID, AreaType>();

        private static string pvpAreaFile;

        public void OnPlayerMoved(Players.Player player, UnityEngine.Vector3 newLocation)
        {
            Vector3Int playerPosition = new Vector3Int(player.Position);
            
            foreach(var area in areas)
            {
                if(area.Contains(playerPosition))
                {
                    if (!playersWithinAnArea.ContainsKey(player.ID) || playersWithinAnArea[player.ID] != area.areaType)
                        Chatting.Chat.Send(player, (area.areaType == AreaType.PvP) ? "You have entered a <color=red>PvP</color> area." : "You have entered a <color=red>Non PvP</color> area.");

                    playersWithinAnArea[player.ID] = area.areaType;

                    return;
                }
            }

            if (playersWithinAnArea.ContainsKey(player.ID))
            {
                Chatting.Chat.Send(player, (playersWithinAnArea[player.ID] == AreaType.PvP) ? "You have left the <color=red>PvP</color> area." : "You have left the <color=red>Non PvP</color> area.");
                playersWithinAnArea.Remove(player.ID);
            }
        }

        public void AfterWorldLoad()
        {
            pvpAreaFile = "./gamedata/savegames/" + ServerManager.WorldName + "/pvparea.json";

            if (!File.Exists(pvpAreaFile))
                return;

            areas = JsonConvert.DeserializeObject<List<Area>>(File.ReadAllText(pvpAreaFile));
        }


        public static void SaveArea()
        {
            string json = JsonConvert.SerializeObject(areas);

            File.WriteAllText(pvpAreaFile, json);
        }

        public void OnAutoSaveWorld()
        {
            SaveArea();
        }

        public void OnQuit()
        {
            SaveArea();
        }
    }
}
