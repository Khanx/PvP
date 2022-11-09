using Chatting;
using NetworkUI;
using System.Collections.Generic;
using ModLoaderInterfaces;
using Newtonsoft.Json.Linq;
using Pipliz;
using System;
using NetworkUI.Items;

namespace PvP
{
    [ChatCommandAutoLoader]
    public class PvPTeam : IChatCommand, IOnPlayerPushedNetworkUIButton
    {
        public static Stack<(DateTime, Players.PlayerIDShort, Players.PlayerIDShort)> killLog = new Stack<(DateTime, Players.PlayerIDShort, Players.PlayerIDShort)>();

        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().Equals("/pvpteam"))
                return false;

            SendTeamMenu(player);

            return true;
        }

        public static void SendTeamMenu(Players.Player player)
        {
            NetworkMenu menu = new NetworkMenu();

            //If player has a team show the interface of his team
            if (TeamMgr.TryGetTeam(player, out Team team))
            {
                menu.LocalStorage.SetAs("header", "Team Manager");
                menu.Width = 550;
                menu.Height = 600;

                if (team.GetLeaderID() == player.ID.ID)
                    menu.Items.Add(new ButtonCallback("PvPTeam_Disband", new LabelData("Disband team", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup));

                menu.Items.Add(new ButtonCallback("PvPTeam_Leave", new LabelData("Leave team", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), onClickActions: NetworkUI.Items.ButtonCallback.EOnClickActions.ClosePopup));

                menu.Items.Add(new EmptySpace(25));

                menu.Items.Add(new Label(new LabelData("Members", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));
                
                Table tableMembers = new Table(550, 180)
                {
                    ExternalMarginHorizontal = 0f
                };
                {
                    var headerRow = new HorizontalRow(new List<(IItem, int)>()
                        {
                            (new Label("Member"), 200),
                             (new Label("Action"), 150)
                        });
                    var headerBG = new BackgroundColor(headerRow, height: -1, color: Table.HEADER_COLOR);
                    tableMembers.Header = headerBG;
                }

                tableMembers.Rows = new List<IItem>();

                foreach (var p in team.players)
                {
                    List<(IItem, int)> t = new List<(IItem, int)>
                    {
                        (new Label(Extender.GetPlayer(p).Name), 200),
                        (new ButtonCallback("PvPTeam_Kick", new LabelData("Kick", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "player", player.ID.ID.ID}}, isInteractive: player.ID.ID == team.GetLeaderID()), 150)
                    };

                    tableMembers.Rows.Add(new HorizontalRow(t));
                }

                menu.Items.Add(tableMembers);
                
                if (team.request.Count > 0 && team.GetLeaderID() == player.ID.ID)
                {
                    menu.Items.Add(new EmptySpace(25));
                    menu.Items.Add(new Label(new LabelData("Join Request", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));

                    Table tableJoinRequest = new Table(550, 180)
                    {
                        ExternalMarginHorizontal = 0f
                    };
                    {
                        var headerRow = new HorizontalRow(new List<(IItem, int)>()
                        {
                            (new Label("Player"), 200),
                            (new Label("Action"), 150),
                            (new Label(""), 150)
                        });
                        var headerBG = new BackgroundColor(headerRow, height: -1, color: Table.HEADER_COLOR);
                        tableJoinRequest.Header = headerBG;
                    }
                    tableJoinRequest.Rows = new List<IItem>();

                    foreach (var p in team.request)
                    {
                        List<(IItem, int)> t = new List<(IItem, int)>
                        {
                            (new Label(Extender.GetPlayer(p).Name), 200),
                            (new ButtonCallback("PvPTeam_JoinAccept", new LabelData("Accept", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "player", player.ID.ID.ID}}, isInteractive: player.ID.ID == team.GetLeaderID()), 150),
                            (new ButtonCallback("PvPTeam_JoinReject", new LabelData("Reject", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "player", player.ID.ID.ID}}, isInteractive: player.ID.ID == team.GetLeaderID()), 150)
                        };

                        tableJoinRequest.Rows.Add(new HorizontalRow(t));
                    }

                    menu.Items.Add(tableJoinRequest);
                }
                
            }
            else
            {
                menu.LocalStorage.SetAs("header", "PvP Teams");
                menu.Width = 550;
                menu.Height = 650;

                menu.Items.Add(new ButtonCallback("PvPTeam_NewTeam", new LabelData("Create Team", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter)));

                if(TeamMgr.Teams.Count > 0)
                {
                    menu.Items.Add(new EmptySpace(50));

                    Table tableTeams = new Table(550, 180)
                    {
                        ExternalMarginHorizontal = 0f
                    };
                    {
                        var headerRow = new HorizontalRow(new List<(IItem, int)>()
                        {
                            (new Label("Team"), 200),
                             (new Label("Action"), 150)
                        });
                        var headerBG = new BackgroundColor(headerRow, height: -1, color: Table.HEADER_COLOR);
                        tableTeams.Header = headerBG;
                    }

                    foreach (var teamF in TeamMgr.Teams)
                    {
                        Players.Player teamLeader = teamF.GetLeader();

                        List<(IItem, int)> t = new List<(IItem, int)>
                    {
                        (new Label(teamLeader.Name), 200),
                        (new ButtonCallback("PvPTeam_JoinRequest", new LabelData("Join", UnityEngine.Color.white, UnityEngine.TextAnchor.MiddleCenter), ButtonPayload: new JObject() { { "player", player.ID.ID.ID}, { "team", teamLeader.ID.ID.ID} }), 150)
                    };

                        tableTeams.Rows.Add(new HorizontalRow(t));
                    }

                    menu.Items.Add(tableTeams);
                }
            }

            NetworkMenuManager.SendServerPopup(player, menu);
        }

        public void OnPlayerPushedNetworkUIButton(ButtonPressCallbackData data)
        {
            switch (data.ButtonIdentifier)
            {
                case "PvPTeam_NewTeam":
                {
                    TeamMgr.CreateTeam(data.Player);
                    SendTeamMenu(data.Player);
                }
                break;

                case "PvPTeam_Kick":
                {
                    if (Players.TryGetPlayer(new Players.PlayerIDShort(data.ButtonPayload.Value<int>("player")), out Players.Player plr))
                    {
                        TeamMgr.KickTeam(plr);
                    }

                    SendTeamMenu(data.Player);
                }
                break;

                case "PvPTeam_Leave":
                {
                    TeamMgr.LeaveTeam(data.Player);
                }
                break;

                case "PvPTeam_Disband":
                {
                    TeamMgr.DisbandTeam(data.Player);
                }
                break;

                case "PvPTeam_JoinRequest":
                {
                    if (Players.TryGetPlayer(new Players.PlayerIDShort(data.ButtonPayload.Value<int>("player")), out Players.Player teamLeader))
                    {
                        TeamMgr.RequestJoinTeam(data.Player, teamLeader);
                    }
                }
                break;

                case "PvPTeam_JoinAccept":
                {
                    if (Players.TryGetPlayer(new Players.PlayerIDShort(data.ButtonPayload.Value<int>("player")), out Players.Player player))
                    {
                        TeamMgr.AcceptJoinTeam(player, data.Player);
                    }
                }
                break;

                case "PvPTeam_JoinReject":
                {
                    if (Players.TryGetPlayer(new Players.PlayerIDShort(data.ButtonPayload.Value<int>("player")), out Players.Player player))
                    {
                        TeamMgr.RejectJoinTeam(player, data.Player);
                    }
                }
                break;
            }
        }
    }
}
