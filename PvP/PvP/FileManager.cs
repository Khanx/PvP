using System.Collections.Generic;
using System.IO;
using ModLoaderInterfaces;
using Newtonsoft.Json;

namespace PvP
{
    public class FileManager : IAfterWorldLoad, IOnAutoSaveWorld, IOnQuit
    {
        private static string pvpAreaFile, pvpBannedFile, pvpSettingsFile, pvpLogFile;

        public void AfterWorldLoad()
        {
            if (!Directory.Exists("./gamedata/savegames/" + ServerManager.WorldName + "/pvp"))
                Directory.CreateDirectory("./gamedata/savegames/" + ServerManager.WorldName + "/pvp");

            pvpSettingsFile = "./gamedata/savegames/" + ServerManager.WorldName + "/pvp/pvpsettings.json";
            pvpBannedFile = "./gamedata/savegames/" + ServerManager.WorldName + "/pvp/pvpbanned.json";
            pvpAreaFile = "./gamedata/savegames/" + ServerManager.WorldName + "/pvp/pvparea.json";
            pvpLogFile = "./gamedata/savegames/" + ServerManager.WorldName + "/pvp/pvplog.json";

            if (File.Exists(pvpSettingsFile))
                PvPManagement.settings = JsonConvert.DeserializeObject<Dictionary<string, int>>(File.ReadAllText(pvpSettingsFile));

            if (File.Exists(pvpBannedFile))
                PvPManagement.LoadBannedPlayers(JsonConvert.DeserializeObject<List<Players.PlayerIDShort>>(File.ReadAllText(pvpBannedFile)));

            if (File.Exists(pvpAreaFile))
                AreaManager.areas = JsonConvert.DeserializeObject<List<Area>>(File.ReadAllText(pvpAreaFile));

            if (File.Exists(pvpLogFile))
                PvPManage.killLog = JsonConvert.DeserializeObject<Stack<(System.DateTime, Players.PlayerIDShort, Players.PlayerIDShort)>>(File.ReadAllText(pvpLogFile));
        }

        private static void SaveData()
        {
            string settingsJson = JsonConvert.SerializeObject(PvPManagement.settings);

            File.WriteAllText(pvpSettingsFile, settingsJson);

            string bannedJson = JsonConvert.SerializeObject(PvPManagement.GetBannedList());

            File.WriteAllText(pvpBannedFile, bannedJson);

            string areaJson = JsonConvert.SerializeObject(AreaManager.areas);

            File.WriteAllText(pvpAreaFile, areaJson);

            string logJson = JsonConvert.SerializeObject(PvPManage.killLog);

            File.WriteAllText(pvpLogFile, logJson);
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
