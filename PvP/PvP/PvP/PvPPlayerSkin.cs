using HarmonyLib;
using MeshedObjects;
using ModLoaderInterfaces;
using Pipliz;
using Shared.Networking;
using System.Collections.Generic;
using UnityEngine;

namespace PvP
{
    [ModLoader.ModManager]
    [HarmonyPatch(typeof(Players.Player), "UpdatePosition")]
    public class PvPPlayerSkin : IAfterWorldLoad
    {
        private static readonly Dictionary<NetworkID, ServerTimeStamp> nextSkin = new Dictionary<NetworkID, ServerTimeStamp>();
        private static readonly long timeBetweenSkins = 950L;

        public static void ChangePlayerSkin(NetworkID playerID)
        {
            nextSkin[playerID] = ServerTimeStamp.Now;
        }

        public static ushort defaultSkin;
        public static ushort pvpSkin;

        public void AfterWorldLoad()
        {
            var harmony = new Harmony("Khanx.ChangeSkin");
            //Harmony.DEBUG = true;
            harmony.PatchAll();
            defaultSkin = NPC.NPCType.GetByKeyNameOrDefault("pipliz.networkplayer").Type;
            pvpSkin = NPC.NPCType.GetByKeyNameOrDefault("khanx.pvp").Type;
        }

        public static bool Prefix(Players.Player __instance, ByteReader data)
        {
            if (nextSkin.TryGetValue(__instance.ID, out ServerTimeStamp time))
            {
                if (time.TimeSinceThis < timeBetweenSkins)
                    return false;
                else
                {
                    nextSkin.Remove(__instance.ID);
                }
            }

            ushort newPlayerSkin = PvPManagement.IsInPvP(__instance.ID) ? pvpSkin : defaultSkin;

            Vector3 v = data.ReadVector3Single();
            uint num = data.ReadVariableUInt();
            uint num2 = data.ReadVariableUInt();
            uint num3 = data.ReadVariableUInt();
            Color32? color = null;
            if (data.ReadBool())
            {
                color = new Color32(data.ReadU8(), data.ReadU8(), data.ReadU8(), byte.MaxValue);
            }

            Vector3 arg = __instance.Position;
            __instance.Position = v;

            //INTERNAL SET
            //__instance.Rotation = Quaternion.Euler(num, num2, num3);
            var Rotation = __instance.GetType().GetProperty("Rotation");
            Rotation.SetValue(__instance, Quaternion.Euler(num, num2, num3), null);

            ModLoader.Callbacks.OnPlayerMoved.Invoke(__instance, arg);

            using (ByteBuilder byteBuilder = ByteBuilder.Get())
            {
                byteBuilder.Write(ClientMessageType.PlayerUpdate);
                __instance.ID.GetBytes(byteBuilder);
                byteBuilder.Write(newPlayerSkin);
                byteBuilder.Write(v);
                byteBuilder.WriteVariable(num);
                byteBuilder.WriteVariable(num2);
                byteBuilder.WriteVariable(num3);
                if (MeshedObjectManager.TryGetVehicle(__instance, out MeshedVehicleDescription description))
                {
                    byteBuilder.Write(b: true);
                    description.WriteTo(byteBuilder);
                }
                else
                {
                    byteBuilder.Write(b: false);
                }

                byteBuilder.Write(color.HasValue);
                if (color.HasValue)
                {
                    Color32 value = color.Value;
                    byteBuilder.Write(value.r);
                    byteBuilder.Write(value.g);
                    byteBuilder.Write(value.b);
                }

                Players.SendToNearbyBut(new Pipliz.Vector3Int(v), byteBuilder, __instance, 150);
            }

            return false;
        }

    }
}
