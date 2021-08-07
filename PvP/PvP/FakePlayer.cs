using Chatting;
using Pipliz;
using Shared.Networking;
using System.Collections.Generic;
using ModLoaderInterfaces;

//This method must be only be available for testing
#if DEBUG
namespace PvP
{
    [ChatCommandAutoLoader]
    public class FakePlayer : IChatCommand, IOnUpdate
    {
        public static Players.Player FKplayer = null;

        private static ServerTimeStamp nextUpdate;
        private static readonly long timeBetweenUpdates = 100L;
        public void OnUpdate()
        {
            if (FKplayer == null)
                return;

            if (nextUpdate == null)
                nextUpdate = ServerTimeStamp.Now;

            if (nextUpdate.TimeSinceThis < timeBetweenUpdates)
                return;

            nextUpdate = ServerTimeStamp.Now;

            FKplayer.Health = FKplayer.HealthMax;
            using (ByteBuilder b = ByteBuilder.Get())
            {
                b.Write(ClientMessageType.PlayerUpdate);
                new NetworkID(new Steamworks.CSteamID(new Steamworks.AccountID_t(0), Steamworks.EUniverse.k_EUniversePublic, Steamworks.EAccountType.k_EAccountTypeAnonUser)).GetBytes(b);

                b.Write(NPC.NPCType.GetByKeyNameOrDefault("pipliz.networkplayer").Type);
                b.Write(FKplayer.Position);
                b.WriteVariable((uint)FKplayer.Rotation.eulerAngles.x);
                b.WriteVariable((uint)FKplayer.Rotation.eulerAngles.y);
                b.WriteVariable((uint)FKplayer.Rotation.eulerAngles.z);

                //NO GliDER
                b.Write(false);
                //NO COLOR
                b.Write(false);

                Players.SendToNearby(new Vector3Int(FKplayer.Position), b, 150);
            }
        }

        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().Equals("/spawnfp"))
                return false;

            if(FKplayer == null)
                FKplayer = Players.GetPlayer(new NetworkID(new Steamworks.CSteamID(new Steamworks.AccountID_t(0), Steamworks.EUniverse.k_EUniversePublic, Steamworks.EAccountType.k_EAccountTypeAnonUser)));

            FKplayer.Name = "Fake Player";
            FKplayer.Position = player.Position;
            FKplayer.ConnectionState = player.ConnectionState;
            FKplayer.Health = FKplayer.HealthMax;

            using (ByteBuilder b = ByteBuilder.Get())
            {
                b.Write(ClientMessageType.PlayerUpdate);
                new NetworkID(new Steamworks.CSteamID(new Steamworks.AccountID_t(0), Steamworks.EUniverse.k_EUniversePublic, Steamworks.EAccountType.k_EAccountTypeAnonUser)).GetBytes(b);

                b.Write(NPC.NPCType.GetByKeyNameOrDefault("pipliz.networkplayer").Type);
                b.Write(player.Position);
                b.WriteVariable((uint)player.Rotation.eulerAngles.x);
                b.WriteVariable((uint)player.Rotation.eulerAngles.y);
                b.WriteVariable((uint)player.Rotation.eulerAngles.z);

                //NO GliDER
                b.Write(false);
                //NO COLOR
                b.Write(false);

                for (int i = 0; i < 4; i++)
                    Players.SendToNearby(new Vector3Int(player.Position), b, 150);
            }

            Chat.Send(player, "Fake player Spawned");
            return true;
        }
    }
}
#endif