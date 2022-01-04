using System;
using System.Collections.Generic;

using Chatting.Commands;

using Pipliz;

using ModLoaderInterfaces;
using BlockEntities;
using BlockEntities.Helpers;
using System.Linq;

namespace PvP
{
    public class RespawnMgr : IOnPlayerRespawn, IAfterAddingBaseTypes
    {
        public void AfterAddingBaseTypes(Dictionary<string, ItemTypesServer.ItemTypeRaw> types)
        {
            for (int i = 0; i < ModLoader.Callbacks.OnPlayerRespawn.CallbackDescriptions.Length; i++)
            {
                ModLoader.ModCallbackDescription callbackD = ModLoader.Callbacks.OnPlayerRespawn.CallbackDescriptions[i];

                if (callbackD.Name.Equals("pipliz.server.onplayerrespawn"))
                {
                    Log.Write("<color=blue>Removed OnPlayerRespawn callback</color>");
                    Action<Players.Player> action = (plr) => { };
                    ModLoader.Callbacks.OnPlayerRespawn.Delegates[i] = action;
                }
            }
            ModLoader.Callbacks.OnPlayerRespawn.Sort();
        }

        public void OnPlayerRespawn(Players.Player player)
        {
            Teleport.TeleportToBase(player);
            Players.SetHealth(player, player.HealthMax, sendToPlayer: true);
            Log.Write("Player {0} respawned.", player);
        }
    }
}
