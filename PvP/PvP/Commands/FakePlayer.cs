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

            //FKplayer.Health = FKplayer.HealthMax;
            using (ByteBuilder b = ByteBuilder.Get())
            {
                b.Write(ClientMessageType.PlayerUpdate);
                new NetworkID(new Steamworks.CSteamID(new Steamworks.AccountID_t(0), Steamworks.EUniverse.k_EUniversePublic, Steamworks.EAccountType.k_EAccountTypeAnonUser)).GetBytes(b);

                b.Write(NPC.NPCType.GetByKeyNameOrDefault("pipliz.merchant").Type);
                b.Write(FKplayer.Position);
                b.WriteVariable((uint)FKplayer.Rotation.eulerAngles.x);
                b.WriteVariable((uint)FKplayer.Rotation.eulerAngles.y);
                b.WriteVariable((uint)FKplayer.Rotation.eulerAngles.z);

                //NO GliDER
                b.Write(false);
                //NO COLOR = b.Write(false); & NOT MORE BYTES
                b.Write(true);
                b.Write(255);
                b.Write(0);
                b.Write(0);

                Players.SendToNearby(new Vector3Int(FKplayer.Position), b, 150);
            }
        }

        public bool TryDoCommand(Players.Player player, string chat, List<string> splits)
        {
            if (!chat.Trim().ToLower().StartsWith("/spawnfp"))
                return false;

            if (FKplayer == null)
                FKplayer = Players.GetPlayer(new NetworkID(new Steamworks.CSteamID(new Steamworks.AccountID_t(0), Steamworks.EUniverse.k_EUniversePublic, Steamworks.EAccountType.k_EAccountTypeAnonUser)));

            FKplayer.Name = "Fake Player";
            FKplayer.Position = player.Position;
            FKplayer.ConnectionState = player.ConnectionState;
            FKplayer.Health = FKplayer.HealthMax;

            using (ByteBuilder b = ByteBuilder.Get())
            {
                b.Write(ClientMessageType.PlayerUpdate);
                new NetworkID(new Steamworks.CSteamID(new Steamworks.AccountID_t(0), Steamworks.EUniverse.k_EUniversePublic, Steamworks.EAccountType.k_EAccountTypeAnonUser)).GetBytes(b);

                b.Write(NPC.NPCType.GetByKeyNameOrDefault("pipliz.merchant").Type);
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

            //clear Inventory
            FKplayer.Inventory.Clear();

            if(splits.Count > 1)
            {
                switch(splits[1])
                {
                    case "cloth":
                        FKplayer.Inventory.TryAdd(PlayerInteraction.armorType[0]);
                        Chat.Send(player, "Fake player equiped with Cloth Armor");
                        break;
                    case "chain":
                        FKplayer.Inventory.TryAdd(PlayerInteraction.armorType[1]);
                        Chat.Send(player, "Fake player equiped with Chain Armor");
                        break;
                    case "plate":
                        FKplayer.Inventory.TryAdd(PlayerInteraction.armorType[2]);
                        Chat.Send(player, "Fake player equiped with Plate Armor");
                        break;
                }
            }

            

            {   //Simulating player movement for the AreaManagement
                Vector3Int playerPosition = new Vector3Int(FKplayer.Position);

                bool fkplayerInArea = false;

                foreach (var area in AreaManager.areas)
                {
                    if (area.Contains(playerPosition))
                    {
                        fkplayerInArea = true;

                        Chatting.Chat.SendToConnected((area.areaType == AreaType.PvP) ? FKplayer.Name + " have entered a <color=red>PvP</color> area." : FKplayer.Name + " have entered a <color=red>Non PvP</color> area.");

                        AreaManager.playersWithinAnArea[FKplayer.ID] = area.areaType;
                        break;
                    }
                }

                if (AreaManager.playersWithinAnArea.ContainsKey(FKplayer.ID) && !fkplayerInArea)
                {
                    Chatting.Chat.SendToConnected((AreaManager.playersWithinAnArea[FKplayer.ID] == AreaType.PvP) ? FKplayer.Name + " have left the <color=red>PvP</color> area." : FKplayer.Name + " have left the <color=red>Non PvP</color> area.");
                    AreaManager.playersWithinAnArea.Remove(FKplayer.ID);
                }
            }

            return true;
        }
    }
}
#endif