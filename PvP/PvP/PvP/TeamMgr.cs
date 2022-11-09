using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvP
{
    public struct Team
    {
        public int leader;
        public List<Players.PlayerIDShort> players;
        public List<Players.PlayerIDShort> request;

        public Team(Players.Player leader) : this()
        {
            this.leader = 0;
            players = new List<Players.PlayerIDShort> { leader.ID.ID };
            request = new List<Players.PlayerIDShort>();
        }

        public Players.Player GetLeader()
        {
            Players.TryGetPlayer(players[leader], out Players.Player player);

            return player;
        }

        public Players.PlayerIDShort GetLeaderID()
        {
            return players[leader];
        }
    }

    public static class TeamMgr
    {
        private static readonly List<Team> teams = new List<Team>();

        public static List<Team> Teams => teams;

        public static void CreateTeam(Players.Player player)
        {
            teams.Add(new Team(player));
            Chatting.Chat.Send(player, "You have created a team, use /pvpteam to manage the team.");
        }

        public static void DisbandTeam(Players.Player player)
        {
            if (TryGetTeam(player, out Team team))
            {
                if (team.players[team.leader] != player.ID.ID)
                {
                    Chatting.Chat.Send(player, "Only the leader of the team can disband the team.");
                    return;
                }

                foreach (var pID in team.players)
                {
                    Chatting.Chat.Send(Extender.GetPlayer(pID), "The team has been disbanded.");
                }

                teams.Remove(team);
                PvPPlayerSkin.ChangePlayerSkin(player.ID.ID);
            }
        }

        public static void RequestJoinTeam(Players.Player player, Players.Player teamLeader)
        {
            if (TryGetTeam(teamLeader, out Team team))
            {
                if (!team.request.Contains(player.ID.ID))
                    team.request.Add(player.ID.ID);
            }

            Chatting.Chat.Send(player, "You have requested to join the team of "+ teamLeader.Name + ".");
            Chatting.Chat.Send(teamLeader, player.Name + " has requested to join your team.");
        }

        public static void RejectJoinTeam(Players.Player player, Players.Player teamLeader)
        {
            if (TryGetTeam(teamLeader, out Team team))
            {
                team.request.Remove(player.ID.ID);

                Chatting.Chat.Send(player, teamLeader.Name + " has rejected your request of joining his/her team.");
            }
        }

        public static void AcceptJoinTeam(Players.Player player, Players.Player teamLeader)
        {
            JoinTeam(player, teamLeader);

            foreach (var t in teams)
                t.request.Remove(player.ID.ID);
        }

        /// <summary>
        /// This method is to add a player to another team WITHOUT requesting
        /// </summary>
        /// <param name="player"></param>
        /// <param name="teamLeader"></param>
        public static void JoinTeam(Players.Player player, Players.Player teamLeader)
        {
            if (TryGetTeam(teamLeader, out Team team))
            {
                foreach (var pID in team.players)
                {
                    Chatting.Chat.Send(Extender.GetPlayer(pID), player.Name + " has joined the team.");
                }

                team.players.Add(player.ID.ID);

                Chatting.Chat.Send(player, "You have joined the team of " + teamLeader.Name);

                PvPPlayerSkin.ChangePlayerSkin(player.ID.ID);

                return;
            }
        }

        public static void LeaveTeam(Players.Player player)
        {
            if (TryGetTeam(player, out Team team))
            {
                if (team.players.Count == 1)
                {
                    DisbandTeam(player);
                    return;
                }

                if (team.players[team.leader] == player.ID.ID)
                {
                    team.leader = 0;    //Sets the new leader
                }

                team.players.Remove(player.ID.ID);

                PvPPlayerSkin.ChangePlayerSkin(player.ID.ID);
            }
        }

        public static void KickTeam(Players.Player player)
        {
            if (TryGetTeam(player, out Team team))
            {
                if (team.players[team.leader] == player.ID.ID) //The leader cannot be kicket
                    return;

                team.players.Remove(player.ID.ID);

                PvPPlayerSkin.ChangePlayerSkin(player.ID.ID);
            }
        }

        public static bool TryGetTeam(Players.Player player, out Team team)
        {
            foreach (var t in teams)
            {
                if (t.players.Contains(player.ID.ID))
                {
                    team = t;
                    return true;
                }
            }

            team = default;
            return false;
        }
    }
}
