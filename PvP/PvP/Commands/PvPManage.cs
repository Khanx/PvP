using Chatting;
using NetworkUI;
using System.Collections.Generic;
using ModLoaderInterfaces;
using Newtonsoft.Json.Linq;
using Pipliz;
using System;

namespace PvP
{
    [ChatCommandAutoLoader]
    public class PvPManage : IChatCommand, IOnPlayerPushedNetworkUIButton
    {
        public static Stack<(DateTime, NetworkID, NetworkID)> killLog = new Stack<(DateTime, NetworkID, NetworkID)>();

        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().Equals("/pvpmanage"))
                return false;

            if (!PermissionsManager.CheckAndWarnPermission(player, "khanx.pvp.manage"))
                return true;

            SendManageMenu(player);

            return true;
        }

        public static void SendManageMenu(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Manage PvP");
            menu.Width = 500;

            menu.Items.Add(new NetworkUI.Items.ButtonCallback("PvPManage_GlobalSettings", new LabelData("Global Settings", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, isInteractive: PermissionsManager.HasPermission(player, "khanx.pvp.global")));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("PvPManage_PlayerList", new LabelData("Manage Players", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("PvPManage_BannedList", new LabelData("Manage Banned Players", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("PvPManage_Log", new LabelData("Log", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup));

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendManageGlobalSettings(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Manage Global Settings");
            menu.Width = 500;

            int status = PvPManagement.settings.GetValueOrDefault("GlobalPvP", 0);

            menu.Items.Add(new NetworkUI.Items.ButtonCallback("PvPManage_GlobalSettingsPvPStatus", new LabelData("Normal PvP", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "status", 0 } }, isInteractive: 0 != status));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("PvPManage_GlobalSettingsPvPStatus", new LabelData("PvP On for everyone", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "status", 1 } }, isInteractive: 1 != status));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("PvPManage_GlobalSettingsPvPStatus", new LabelData("PvP Off for everyone", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "status", 2 } }, isInteractive: 2 != status));

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendManagePlayerList(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Manage PvP Players");
            menu.Width = 650;
            menu.Height = 600;

            NetworkUI.Items.Table table = new NetworkUI.Items.Table(650, 500)
            {
                ExternalMarginHorizontal = 0f
            };

            {
                var headerRow = new NetworkUI.Items.HorizontalRow(new List<(IItem, int)>()
                {
                    (new NetworkUI.Items.Label("Name"), 250),
                    (new NetworkUI.Items.Label("Status"), 100),     //PvP On, PvP Off, Staff
                    (new NetworkUI.Items.EmptySpace(), 100),     //Enable / Disable PvP
                    (new NetworkUI.Items.EmptySpace(), 150)     //Ban from PvP
                });
                var headerBG = new NetworkUI.Items.BackgroundColor(headerRow, height: -1, color: NetworkUI.Items.Table.HEADER_COLOR);
                table.Header = headerBG;
            }

            table.Rows = new List<IItem>();

            for (int i = 0; i < Players.CountConnected; i++)
            {
                Players.Player plr = Players.GetConnectedByIndex(i);
                bool staffMember = PermissionsManager.HasPermission(plr, "khanx.pvp");

                List<(IItem, int)> row = new List<(IItem, int)>();
                row.Add((new NetworkUI.Items.Label(plr.Name), 250));

                if (staffMember)
                {
                    row.Add((new NetworkUI.Items.Label(new LabelData("Staff", UnityEngine.Color.blue)), 100));
                }
                else if (PvPManagement.IsBanned(plr.ID))
                {
                    row.Add((new NetworkUI.Items.Label(new LabelData("BANNED", UnityEngine.Color.yellow)), 100));
                }
                else if (PvPManagement.HasPvPEnabled(plr.ID))
                {
                    row.Add((new NetworkUI.Items.Label(new LabelData("PvP On", UnityEngine.Color.red)), 100));
                }
                else
                {
                    row.Add((new NetworkUI.Items.Label(new LabelData("PvP Off", UnityEngine.Color.green)), 100));
                }

                row.Add((new NetworkUI.Items.ButtonCallback("PvPManage_ChangePvPStatus", new LabelData((PvPManagement.HasPvPEnabled(plr.ID)) ? "Disable PvP" : "Enable PvP", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "player", plr.ID.ToString() } }, isInteractive: !staffMember), 100));
                row.Add((new NetworkUI.Items.ButtonCallback("PvPManage_ChangeBanStatus", new LabelData((PvPManagement.IsBanned(plr.ID) ? "Ban from PvP" : "Unban from PvP"), UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "player", plr.ID.ToString() }, { "returnPlayerList", true } }, isInteractive: !staffMember), 150));

                table.Rows.Add(new NetworkUI.Items.HorizontalRow(row));
            }

            menu.Items.Add(table);

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendManageBannedList(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "Manage Banned Players");
            menu.Width = 500;
            menu.Height = 600;

            NetworkUI.Items.Table table = new NetworkUI.Items.Table(650, 450)
            {
                ExternalMarginHorizontal = 0f
            };

            {
                var headerRow = new NetworkUI.Items.HorizontalRow(new List<(IItem, int)>()
                {
                    (new NetworkUI.Items.Label("Name"), 250),
                    (new NetworkUI.Items.EmptySpace(), 150)     //UnBan from PvP
                });
                var headerBG = new NetworkUI.Items.BackgroundColor(headerRow, height: -1, color: NetworkUI.Items.Table.HEADER_COLOR);
                table.Header = headerBG;
            }

            table.Rows = new List<IItem>();

            foreach (var plrID in PvPManagement.GetBannedList())
            {
                if (Players.PlayerDatabase.TryGetValue(plrID, out Players.Player plr))
                {
                    List<(IItem, int)> row = new List<(IItem, int)>
                    {
                        (new NetworkUI.Items.Label(plr.Name), 250),
                        (new NetworkUI.Items.ButtonCallback("PvPManage_UnBanPlayer", new LabelData("Unban from PvP", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "player", plr.ID.ToString() } }), 150)
                    };

                    table.Rows.Add(new NetworkUI.Items.HorizontalRow(row));
                }
            }

            menu.Items.Add(table);

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public static void SendPvPLog(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();
            menu.LocalStorage.SetAs("header", "PvP Log");
            menu.Width = 800;
            menu.Height = 600;

            NetworkUI.Items.Table table = new NetworkUI.Items.Table(800, 450)
            {
                ExternalMarginHorizontal = 0f
            };

            {
                var headerRow = new NetworkUI.Items.HorizontalRow(new List<(IItem, int)>()
                {
                    (new NetworkUI.Items.Label("Time"), 250),
                    (new NetworkUI.Items.Label("Killer"), 250),
                    (new NetworkUI.Items.Label("Killed"), 250)
                });
                var headerBG = new NetworkUI.Items.BackgroundColor(headerRow, height: -1, color: NetworkUI.Items.Table.HEADER_COLOR);
                table.Header = headerBG;
            }

            table.Rows = new List<IItem>();

            foreach (var klog in killLog)
            {
                if (Players.TryGetPlayer(klog.Item2, out Players.Player killer))
                {
                    if (Players.TryGetPlayer(klog.Item2, out Players.Player killed))
                    {
                        List<(IItem, int)> row = new List<(IItem, int)>
                        {
                            (new NetworkUI.Items.Label(klog.Item1.ToString(System.Globalization.CultureInfo.InvariantCulture)), 250),
                            (new NetworkUI.Items.Label(killer.Name), 250),
                            (new NetworkUI.Items.Label(killed.Name), 250),
                        };
                        table.Rows.Add(new NetworkUI.Items.HorizontalRow(row));
                    }
                }
            }

            menu.Items.Add(table);

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public void OnPlayerPushedNetworkUIButton(ButtonPressCallbackData data)
        {
            switch (data.ButtonIdentifier)
            {
                case "PvPManage_GlobalSettings":
                {
                    SendManageGlobalSettings(data.Player);
                }
                break;

                case "PvPManage_GlobalSettingsPvPStatus":
                {
                    int newStatus = data.ButtonPayload.Value<int>("status");
                    PvPManagement.settings["GlobalPvP"] = newStatus;

                    switch (newStatus)
                    {
                        case 0:
                            Chat.SendToConnected("Now players can decide his PvP Status");

                            for (int i = 0; i < Players.CountConnected; i++)
                            {
                                Players.Player plr = Players.GetConnectedByIndex(i);
                                if (!PermissionsManager.HasPermission(plr, "khanx.pvp"))
                                    PvPManagement.DisablePvP(plr.ID, true);
                            }

                            break;
                        case 1:
                            Chat.SendToConnected("PvP has been enabled for everyone.");

                            for (int i = 0; i < Players.CountConnected; i++)
                            {
                                Players.Player plr = Players.GetConnectedByIndex(i);
                                if (!PermissionsManager.HasPermission(plr, "khanx.pvp"))
                                    PvPManagement.EnablePvP(plr.ID);
                            }

                            break;
                        case 2:
                            Chat.SendToConnected("PvP has been disabled for everyone.");

                            for (int i = 0; i < Players.CountConnected; i++)
                            {
                                Players.Player plr = Players.GetConnectedByIndex(i);
                                if (!PermissionsManager.HasPermission(plr, "khanx.pvp"))
                                    PvPManagement.DisablePvP(plr.ID, true);
                            }

                            break;
                    }

                    SendManageGlobalSettings(data.Player);
                }
                break;

                case "PvPManage_PlayerList":
                {
                    SendManagePlayerList(data.Player);
                }
                break;

                case "PvPManage_ChangePvPStatus":
                {
                    NetworkID plrId = NetworkID.Parse(data.ButtonPayload.Value<string>("player"));

                    if (PvPManagement.HasPvPEnabled(plrId))
                        PvPManagement.DisablePvP(plrId, true);
                    else
                        PvPManagement.EnablePvP(plrId);

                    SendManagePlayerList(data.Player);
                }
                break;

                case "PvPManage_ChangeBanStatus":
                {
                    NetworkID plrId = NetworkID.Parse(data.ButtonPayload.Value<string>("player"));

                    if (PvPManagement.IsBanned(plrId))
                        PvPManagement.UnBanFromPvP(plrId);
                    else
                        PvPManagement.BanFromPvP(plrId);

                    if (data.ButtonPayload.GetAsOrDefault("returnPlayerList", false))
                        SendManagePlayerList(data.Player);
                    else
                        SendManageBannedList(data.Player);
                }
                break;

                case "PvPManage_BannedList":
                {
                    SendManageBannedList(data.Player);
                }
                break;

                case "PvPManage_Log":
                {
                    SendPvPLog(data.Player);
                }
                break;
            }
        }
    }
}
