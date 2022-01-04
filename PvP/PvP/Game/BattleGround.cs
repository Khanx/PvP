using System.Collections.Generic;

namespace PvP
{
    public enum BattleGroundStatus
    {
        STATUS_WAIT             = 0,        // first status, should mean bg is not instance
        STATUS_PREPARE          = 1,        // means bg is empty and waiting for queue
        STATUS_IN_PROGRESS      = 2,        // means bg is running
        STATUS_END              = 3         // means some faction has won BG and it is ending
    }

    public enum BattleGroundTimers : long
    {
        BATTLE_START        = 120000L,      //Time between enters a player & Battle Starts
        BATTLE_DURATION     = 1800000L,     //Battle Duration
        BATTLE_END          = 120000L       //Battle End
    }

    public enum BattleGroundTeam
    {
        TEAM_A,
        TEAM_B
    }

    public struct PlayerBattleground
    {
        public NetworkID playerID;
        public BattleGroundTeam team;
        public int Kills;
        public int Deaths;

        public PlayerBattleground(NetworkID playerID, BattleGroundTeam team) : this()
        {
            this.playerID = playerID;
            this.team = team;
            Kills = 0;
            Deaths = 0;
        }
    }

    public abstract class BattleGround : ModLoaderInterfaces.IOnUpdate
    {
        //MESSAGES
        public readonly string BATTLEGROUND_JOIN    = "You have entered a BattleGround";
        public readonly string BATTLEGROUND_4MIN    = "The BattleGround will start in 4 mins";
        public readonly string BATTLEGROUND_2MIN    = "The BattleGround will start in 2 mins";
        public readonly string BATTLEGROUND_START   = "The BattleGround Starts";
        public readonly string BATTLEGROUND_END     = "The BattleGround Ends";

        public readonly List<PlayerBattleground> players = new List<PlayerBattleground>();

        public BattleGroundStatus status = BattleGroundStatus.STATUS_WAIT;

        /// <summary>
        /// Initialization & reset of variables
        /// </summary>
        public abstract void Prepare();

        public void ForceStart()
        {
            status = BattleGroundStatus.STATUS_IN_PROGRESS;
            Start();
        }

        /// <summary>
        /// The battle starts after X time
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// It is called every second while the battle is ongoing
        /// </summary>
        public abstract void Update();

        public void ForceEnd()
        {
            status = BattleGroundStatus.STATUS_END;
            SendMessageToPlayers(BATTLEGROUND_END);
            SendMessageToPlayers(BATTLEGROUND_4MIN);
            status = BattleGroundStatus.STATUS_IN_PROGRESS;
            SendMessageToPlayers(BATTLEGROUND_START);
            End();
        }

        /// <summary>
        /// It is called when the battle ends
        /// </summary>
        public abstract void End();

        private ServerTimeStamp nextUpdate;
        private static readonly long timeBetweenUpdates = 1000L;    //1 second
        private ServerTimeStamp Timer;

        public void OnUpdate()
        {
            if (status == BattleGroundStatus.STATUS_WAIT)
                return;

            if (nextUpdate == null)
                nextUpdate = ServerTimeStamp.Now;

            if (nextUpdate.TimeSinceThis < timeBetweenUpdates)
                return;

            nextUpdate = ServerTimeStamp.Now;

            if (Timer == null)
                Timer = ServerTimeStamp.Now;

            if(status == BattleGroundStatus.STATUS_PREPARE && Timer.TimeSinceThis > (long) BattleGroundTimers.BATTLE_START)
            {
                status = BattleGroundStatus.STATUS_IN_PROGRESS;
                SendMessageToPlayers(BATTLEGROUND_START);
                Start();
            }

            if (status == BattleGroundStatus.STATUS_IN_PROGRESS && Timer.TimeSinceThis > (long)BattleGroundTimers.BATTLE_DURATION)
            {
                status = BattleGroundStatus.STATUS_END;
                SendMessageToPlayers(BATTLEGROUND_END);
                SendMessageToPlayers(BATTLEGROUND_4MIN);
                End();
            }

            if (status == BattleGroundStatus.STATUS_END && Timer.TimeSinceThis > (long)BattleGroundTimers.BATTLE_END)
            {
                status = BattleGroundStatus.STATUS_PREPARE;
                SendMessageToPlayers(BATTLEGROUND_2MIN);
                Prepare();
            }

            Update();
        }

        public void SendMessageToPlayers(string text)
        {
            foreach(var p in players)
            {
                Chatting.Chat.Send(Players.GetPlayer(p.playerID), text);
            }
        }

        public void OnAddPlayer(Players.Player player)
        {
            Chatting.Chat.Send(player, BATTLEGROUND_JOIN);

            if (status == BattleGroundStatus.STATUS_WAIT)
            {
                SendMessageToPlayers(BATTLEGROUND_2MIN);
                Prepare();
            }
            else
                Chatting.Chat.Send(player, BATTLEGROUND_2MIN);
        }

        public void OnRemovePlayer()
        {
            if (players.Count == 0)
            {
                status = BattleGroundStatus.STATUS_WAIT;
            }
        }

        public bool IsInGame(Players.Player player)
        {
            return players.Exists(plr => plr.playerID == player.ID);
        }

        public List<PlayerBattleground> GetTeam(BattleGroundTeam team)
        {
            return players.FindAll(x => x.team == team);
        }
    }
}
