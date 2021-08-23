using Chatting;
using NetworkUI;
using System.Collections.Generic;
using ModLoaderInterfaces;
using Newtonsoft.Json.Linq;

namespace PvP.Commands
{
    [ChatCommandAutoLoader]
    public class PvPManage : IChatCommand, IOnPlayerPushedNetworkUIButton
    {
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

            menu.Items.Add(new NetworkUI.Items.ButtonCallback("PvPManage_GlobalSettings", new LabelData("Global Settings", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, isInteractive: false));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("PvPManage_PlayerList", new LabelData("Manage Players", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("PvPManage_BannedList", new LabelData("Manage Bans", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, isInteractive: false));
            menu.Items.Add(new NetworkUI.Items.ButtonCallback("PvPManage_Log", new LabelData("Log", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, isInteractive: false));

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
                else if (PvPManagement.pvpPlayers.ContainsKey(plr.ID))
                {
                    row.Add((new NetworkUI.Items.Label(new LabelData("PvP On", UnityEngine.Color.red)), 100));
                }
                else
                {
                    row.Add((new NetworkUI.Items.Label(new LabelData("PvP Off", UnityEngine.Color.green)), 100));
                }

                row.Add((new NetworkUI.Items.ButtonCallback("PvPManage_ChangePvPStatus", new LabelData((PvPManagement.pvpPlayers.ContainsKey(plr.ID)) ? "Disable PvP" : "Enable PvP", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, isInteractive: !staffMember), 100));
                row.Add((new NetworkUI.Items.ButtonCallback("PvPManage_BanPlayer", new LabelData("Ban from PvP", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup, ButtonPayload: new JObject() { { "player", plr.ID.ToString() } }, isInteractive: !staffMember), 150));

                table.Rows.Add(new NetworkUI.Items.HorizontalRow(row));
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
                    Chat.Send(data.Player, data.ButtonIdentifier);
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

                    if (!PvPManagement.pvpPlayers.Remove(plrId))
                    {
                        PvPManagement.pvpPlayers.Add(plrId, ServerTimeStamp.Now);
                    }

                    SendManagePlayerList(data.Player);
                }
                break;

                case "PvPManage_BanPlayer":
                {
                    NetworkID plrId = NetworkID.Parse(data.ButtonPayload.Value<string>("player"));

                    PvPManagement.pvpPlayers.Remove(plrId);

                    //Add to ban list

                    SendManagePlayerList(data.Player);
                }
                break;

                case "PvPManage_BannedList":
                {
                    Chat.Send(data.Player, data.ButtonIdentifier);
                }
                break;

                case "PvPManage_Log":
                {
                    Chat.Send(data.Player, data.ButtonIdentifier);
                }
                break;
            }
        }
    }
}
