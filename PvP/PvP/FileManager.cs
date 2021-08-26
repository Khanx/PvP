using System.Collections.Generic;
using System.IO;
using colonyserver.Assets.UIGeneration;
using ModLoaderInterfaces;
using Newtonsoft.Json;
using Pipliz;

namespace PvP
{
    public class FileManager : IAfterWorldLoad, IOnAutoSaveWorld, IOnQuit
    {
        private static string pvpAreaFile, pvpBannedFile, pvpSettingsFile;

        public void AfterWorldLoad()
        {
            if (!Directory.Exists("./gamedata/savegames/" + ServerManager.WorldName + "/pvp"))
                Directory.CreateDirectory("./gamedata/savegames/" + ServerManager.WorldName + "/pvp");

            pvpSettingsFile = "./gamedata/savegames/" + ServerManager.WorldName + "/pvp/pvpsettings.json";
            pvpBannedFile = "./gamedata/savegames/" + ServerManager.WorldName + "/pvp/pvpbanned.json";
            pvpAreaFile = "./gamedata/savegames/" + ServerManager.WorldName + "/pvp/pvparea.json";

            if (File.Exists(pvpSettingsFile))
                PvPManagement.settings = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(pvpSettingsFile));

            if (File.Exists(pvpBannedFile))
                PvPManagement.bannedPlayers = JsonConvert.DeserializeObject<List<NetworkID>>(File.ReadAllText(pvpBannedFile));

            if (!File.Exists(pvpAreaFile))
                AreaManager.areas = JsonConvert.DeserializeObject<List<Area>>(File.ReadAllText(pvpAreaFile));
        }

        private static void SaveData()
        {
            string settingsJson = JsonConvert.SerializeObject(PvPManagement.settings);

            File.WriteAllText(pvpSettingsFile, settingsJson);

            string bannedJson = JsonConvert.SerializeObject(PvPManagement.bannedPlayers);

            File.WriteAllText(pvpBannedFile, bannedJson);

            string areaJson = JsonConvert.SerializeObject(AreaManager.areas);

            File.WriteAllText(pvpAreaFile, areaJson);
        }

        public void OnAutoSaveWorld()
        {
            SaveData();
        }

        public void OnQuit()
        {
            SaveData();
        }
    }
}
