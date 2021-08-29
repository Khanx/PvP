using Shared;
using System.Collections.Generic;
using UnityEngine;
using ModLoaderInterfaces;

namespace PvP
{
    public enum Weapon
    {
        Punch,
        BronzeSword,
        IronSword,
        SteelSword,
        BronzeSpear,
        IronSpear,
        SteelSpear,
        BronzeMace,
        IronMace,
        SteelMace,
        Sling,
        Bow,
        Crossbow,
        Matchlockgun,
        MAX
    }

    public class PlayerInteraction : IOnPlayerClicked, IAfterWorldLoad, IOnUpdate
    {
        public void OnPlayerClicked(Players.Player player, PlayerClickedData data)
        {
            if (data.ClickType != PlayerClickedData.EClickType.Left)
                return;

            if (!PvPManagement.IsInPvP(player.ID))
            {
#if DEBUG
                Chatting.Chat.SendToConnected(player.Name + " don't have PvP enabled.");
#endif
                return;
            }

            if (data.IsConsumed && data.ConsumedType == PlayerClickedData.EConsumedType.UsedAsTool)
                TryShoot(player, data);
            else
                TryMelee(player, data);
        }

        //READ: https://discord.com/channels/345192439323033601/345214873082527756/835464495744417812

        public static readonly long[] TimeBetweenAttacks = new long[(int)Weapon.MAX];
        //Official damange: PUNCH = 35, SLING = 50, BOW = 100, CROSSBOW = 300, MATCHLOCK = 500
        public static readonly float[] AttackDamage = new float[(int)Weapon.MAX];
        public static readonly ushort[] armorType = new ushort[4];
        public static readonly Dictionary<ushort, Weapon> type2Weapon = new Dictionary<ushort, Weapon>(9);

        public void AfterWorldLoad()
        {
            TimeBetweenAttacks[(int)Weapon.Punch]           = 500L;

            TimeBetweenAttacks[(int)Weapon.BronzeSword]     = 500L;
            TimeBetweenAttacks[(int)Weapon.IronSword]       = 500L;
            TimeBetweenAttacks[(int)Weapon.SteelSword]      = 500L;

            TimeBetweenAttacks[(int)Weapon.BronzeSpear]     = 500L;
            TimeBetweenAttacks[(int)Weapon.IronSpear]       = 500L;
            TimeBetweenAttacks[(int)Weapon.SteelSpear]      = 500L;

            TimeBetweenAttacks[(int)Weapon.BronzeMace]      = 1000L;
            TimeBetweenAttacks[(int)Weapon.IronMace]        = 1000L;
            TimeBetweenAttacks[(int)Weapon.SteelMace]       = 1000L;

            //The time of the ranged weapon cannot be modified
            TimeBetweenAttacks[(int)Weapon.Sling]           = 1000L;
            TimeBetweenAttacks[(int)Weapon.Bow]             = 1500L;
            TimeBetweenAttacks[(int)Weapon.Crossbow]        = 2500L;
            TimeBetweenAttacks[(int)Weapon.Matchlockgun]    = 3000L;

            AttackDamage[(int)Weapon.Punch]                 = 20f;

            AttackDamage[(int)Weapon.BronzeSword]           = 50f;
            AttackDamage[(int)Weapon.IronSword]             = 75f;
            AttackDamage[(int)Weapon.SteelSword]            = 100f;

            AttackDamage[(int)Weapon.BronzeSpear]           = 25f;
            AttackDamage[(int)Weapon.IronSpear]             = 50f;
            AttackDamage[(int)Weapon.SteelSpear]            = 75f;

            AttackDamage[(int)Weapon.BronzeMace]            = 100f;
            AttackDamage[(int)Weapon.IronMace]              = 150f;
            AttackDamage[(int)Weapon.SteelMace]             = 200f;

            AttackDamage[(int)Weapon.Sling]                 = 50f;
            AttackDamage[(int)Weapon.Bow]                   = 75f;
            AttackDamage[(int)Weapon.Crossbow]              = 150f;
            AttackDamage[(int)Weapon.Matchlockgun]          = 200f;

            armorType[0] = ItemTypes.IndexLookup.GetIndex("Khanx.PvPClothArmor");
            armorType[1] = ItemTypes.IndexLookup.GetIndex("Khanx.PvPChainArmor");
            armorType[2] = ItemTypes.IndexLookup.GetIndex("Khanx.PvPPlateArmor");

            type2Weapon.Add(ItemTypes.IndexLookup.GetIndex("Khanx.PvPBronzeSword"), Weapon.BronzeSword);
            type2Weapon.Add(ItemTypes.IndexLookup.GetIndex("Khanx.PvPIronSword"), Weapon.IronSword);
            type2Weapon.Add(ItemTypes.IndexLookup.GetIndex("Khanx.PvPSteelSword"), Weapon.SteelSword);

            type2Weapon.Add(ItemTypes.IndexLookup.GetIndex("Khanx.PvPBronzeSpear"), Weapon.BronzeSpear);
            type2Weapon.Add(ItemTypes.IndexLookup.GetIndex("Khanx.PvPIronSpear"), Weapon.IronSpear);
            type2Weapon.Add(ItemTypes.IndexLookup.GetIndex("Khanx.PvPSteelSpear"), Weapon.SteelSpear);

            type2Weapon.Add(ItemTypes.IndexLookup.GetIndex("Khanx.PvPBronzeMace"), Weapon.BronzeMace);
            type2Weapon.Add(ItemTypes.IndexLookup.GetIndex("Khanx.PvPIronMace"), Weapon.IronMace);
            type2Weapon.Add(ItemTypes.IndexLookup.GetIndex("Khanx.PvPSteelMace"), Weapon.SteelMace);
        }

        public static Dictionary<(Players.Player, Weapon), ServerTimeStamp> LastShoot = new Dictionary<(Players.Player, Weapon), ServerTimeStamp>();

        public static void TryShoot(Players.Player player, PlayerClickedData data)
        {
            if (data.TypeSelected == BlockTypes.BuiltinBlocks.Indices.sling)
            {
                //TIME BETWEEM shoots
                if (LastShoot.TryGetValue((player, Weapon.Sling), out ServerTimeStamp value) && value.TimeSinceThis < TimeBetweenAttacks[(int)Weapon.Sling])
                    return;

                LastShoot[(player, Weapon.Sling)] = ServerTimeStamp.Now;

                projectiles.Add(new Projectile(ProjectileType.Sling, player.Position + Vector3.up, data.PlayerAimDirection, player.ID));
#if DEBUG
                Chatting.Chat.SendToConnected("Sling shoot: " + player.Position);
#endif
            }
            else if (data.TypeSelected == BlockTypes.BuiltinBlocks.Indices.bow)
            {
                //TIME BETWEEM shoots
                if (LastShoot.TryGetValue((player, Weapon.Bow), out ServerTimeStamp value) && value.TimeSinceThis < TimeBetweenAttacks[(int)Weapon.Bow])
                    return;

                LastShoot[(player, Weapon.Bow)] = ServerTimeStamp.Now;

                projectiles.Add(new Projectile(ProjectileType.Arrow, player.Position + Vector3.up, data.PlayerAimDirection, player.ID));
#if DEBUG
                Chatting.Chat.SendToConnected("Bow shoot: " + player.Position);
#endif
            }
            else if (data.TypeSelected == BlockTypes.BuiltinBlocks.Indices.crossbow)
            {
                //TIME BETWEEM shoots
                if (LastShoot.TryGetValue((player, Weapon.Crossbow), out ServerTimeStamp value) && value.TimeSinceThis < TimeBetweenAttacks[(int)Weapon.Crossbow])
                    return;

                LastShoot[(player, Weapon.Crossbow)] = ServerTimeStamp.Now;

                projectiles.Add(new Projectile(ProjectileType.Crossbow, player.Position + Vector3.up, data.PlayerAimDirection, player.ID));
#if DEBUG
                Chatting.Chat.SendToConnected("Crossbow shoot: " + player.Position);
#endif
            }
            else if (data.TypeSelected == BlockTypes.BuiltinBlocks.Indices.matchlockgun)
            {
                //TIME BETWEEM shoots
                if (LastShoot.TryGetValue((player, Weapon.Matchlockgun), out ServerTimeStamp value) && value.TimeSinceThis < TimeBetweenAttacks[(int)Weapon.Matchlockgun])
                    return;

                LastShoot[(player, Weapon.Matchlockgun)] = ServerTimeStamp.Now;

                projectiles.Add(new Projectile(ProjectileType.Matchlock, player.Position + Vector3.up, data.PlayerAimDirection, player.ID));
#if DEBUG
                Chatting.Chat.SendToConnected("Matchlockgun shoot: " + player.Position);
#endif
            }
        }

        public static void TryMelee(Players.Player player, PlayerClickedData data)
        {
            if (!type2Weapon.TryGetValue(data.TypeSelected, out Weapon weapon))
                weapon = Weapon.Punch;

            //TIME BETWEEM shoots
            if (LastShoot.TryGetValue((player, weapon), out ServerTimeStamp value) && value.TimeSinceThis < TimeBetweenAttacks[(int)weapon])
                return;

            LastShoot[(player, weapon)] = ServerTimeStamp.Now;

            Ray ray = new Ray(data.PlayerEyePosition, data.PlayerAimDirection);
#if DEBUG
            ServerManager.SendParticleTrail(data.PlayerEyePosition + data.PlayerAimDirection * 3, data.PlayerEyePosition - data.PlayerAimDirection * 3, 5);

            foreach (Players.Player pl in Players.PlayerDatabase.Values)
            {
#else
            for (int i = 0; i < Players.CountConnected; i++)
            {
                Players.Player pl = Players.GetConnectedByIndex(i);
#endif
                //A plater cannot hit himself
                if (pl.Equals(player))
                    continue;

                int hitDistance = 2;

                //spear can attack from further
                if (weapon == Weapon.BronzeSpear || weapon == Weapon.IronSpear || weapon == Weapon.SteelSpear)
                    hitDistance = 4;

                if (Vector3.Distance(pl.Position, player.Position) > hitDistance)
                    continue;

                Bounds playerBounds = new Bounds(pl.Position + new Vector3(0, 0.5f, 0), new Vector3(1.25f, 2.25f, 0.5f));

                if (playerBounds.IntersectRay(ray))
                {
#if DEBUG
                    Chatting.Chat.SendToConnected(player.Name + " hits " + pl.Name);
#endif
                    AttackPlayer(pl, player.ID, AttackDamage[(int)weapon]);
                    break;
                }
            }
        }

        public static List<Projectile> projectiles = new List<Projectile>();
        private static ServerTimeStamp nextUpdate;
        private static readonly long timeBetweenUpdates = 200L;

        //This method detects projectile collision
        public void OnUpdate()
        {
            if (projectiles.Count == 0)
                return;

            if (nextUpdate == null)
                nextUpdate = ServerTimeStamp.Now;

            if (nextUpdate.TimeSinceThis < timeBetweenUpdates)
                return;

            nextUpdate = ServerTimeStamp.Now;

            /*  Explanation:
                1- Check block collision
                2- Check monster collision
                3- Check colonist collision
                4- Check player collision

                //Player collision DON'T remove the projectile
             */
            for (int i = projectiles.Count - 1; i >= 0; i--)
            {
                Projectile projectile = projectiles[i];

                float time = projectile.shootTimeMS.TimeSinceThis / 1000f;

                Vector3 nextPosition = projectile.startPostion + projectile.velocity * time + 0.5f * Vector3.down * 9.81f * time * time;
#if DEBUG
                ServerManager.SendParticleTrail(projectile.lastPosition, nextPosition, 5);
#endif
                //Check if HITS a type (block)
                //Zun recommends: if (!VoxelPhysics.CanSee(position, nextPosition)) { ... hit something ... }
                if (!VoxelPhysics.CanSee(projectile.lastPosition, nextPosition))
                {
#if DEBUG
                    Chatting.Chat.SendToConnected("Hits Block");
#endif
                    projectiles.RemoveAt(i);
                    continue;
                }

                /*
                if (!World.TryGetTypeAt(Vector3Int.RoundToInt(nextPosition), out ushort typePosition) || typePosition != BlockTypes.BuiltinBlocks.Indices.air)
                {
                    Chatting.Chat.SendToConnected("Hits Block " + ItemTypes.IndexLookup.GetName(typePosition));
                    ProjectileManager.projectiles.RemoveAt(i);
                    continue;
                }
                */

                float distanceBetweenShots = Vector3.Distance(projectile.lastPosition, nextPosition);
                Ray ray = new Ray(projectile.lastPosition, nextPosition - projectile.lastPosition);

                //Check if HITS a Monster
                Monsters.IMonster monster = Monsters.MonsterTracker.Find(Vector3Int.RoundToInt(nextPosition), (int)(2 + distanceBetweenShots), 1);

                if (monster != null)
                {
                    Bounds monsterBounds = new Bounds(monster.Position + new Vector3(0, 0.5f, 0), new Vector3(1.25f, 2.25f, 0.5f));

                    if (monsterBounds.IntersectRay(ray))
                    {
#if DEBUG
                        Chatting.Chat.SendToConnected("Hits Zombie: " + monster.Position);
#endif
                        projectiles.RemoveAt(i);
                        continue;
                    }
                }

                //Check if HITS a NPC
                if (NPC.NPCTracker.TryGetNear(nextPosition, (int)(3 + distanceBetweenShots), out NPC.NPCBase npc))
                {
                    Bounds npcBounds = new Bounds(new Vector3(npc.Position.x, npc.Position.y, npc.Position.z) + new Vector3(0, 0.5f, 0), new Vector3(1.25f, 2.25f, 0.5f));

                    if (npcBounds.IntersectRay(ray))
                    {
#if DEBUG
                        Chatting.Chat.SendToConnected("Hits NPC: " + npc.Position);
#endif

                        projectiles.RemoveAt(i);
                        continue;
                    }
                }
                //Check if HITS a player
                bool hitPlayer = false;
#if DEBUG
                foreach (Players.Player pl in Players.PlayerDatabase.Values)
                {
#else
                    for (int j = 0; j < Players.CountConnected; j++)
                {
                        Players.Player pl = Players.GetConnectedByIndex(j);
#endif
                    //A player cannot shoot himself
                    if (pl.ID.Equals(projectile.shooter))
                        continue;

                    if (Vector3.Distance(pl.Position, nextPosition) > (2 + distanceBetweenShots))
                        continue;

                    Bounds playerBounds = new Bounds(pl.Position + new Vector3(0, 0.5f, 0), new Vector3(1.25f, 2.25f, 0.5f));

                    if (playerBounds.IntersectRay(ray))
                    {
#if DEBUG
                        Chatting.Chat.SendToConnected(Players.GetPlayer(projectile.shooter).Name + " shoots " + pl.Name);
#endif
                        switch (projectile.projectileType)
                        {
                            case ProjectileType.Sling: AttackPlayer(pl, projectile.shooter, AttackDamage[(int)Weapon.Sling]); break;
                            case ProjectileType.Arrow: AttackPlayer(pl, projectile.shooter, AttackDamage[(int)Weapon.Bow]); break;
                            case ProjectileType.Crossbow: AttackPlayer(pl, projectile.shooter, AttackDamage[(int)Weapon.Crossbow]); break;
                            case ProjectileType.Matchlock: AttackPlayer(pl, projectile.shooter, AttackDamage[(int)Weapon.Matchlockgun]); break;
                        }

                        hitPlayer = true;
                        break;
                    }
                }

                if (hitPlayer)
                {
                    projectiles.RemoveAt(i);
                }
                else
                {
                    projectile.lastPosition = nextPosition;
                    projectiles[i] = projectile;
                }
            }
        }

        public static void AttackPlayer(Players.Player attacked, NetworkID attacker, float damage)
        {
            if (!PvPManagement.IsInPvP(attacked.ID))
            {
#if DEBUG
                Chatting.Chat.SendToConnected(attacked.Name + " don't have PvP enabled.");
#endif
                return;
            }

            float damageModifier = 1;

            foreach (var i in attacked.Inventory.Items)
            {
                if (i.Type == armorType[0]) //PvPClothArmor
                {
                    if (damageModifier == 1)
                        damageModifier = 0.75f;
                }
                else if (i.Type == armorType[1])  //PvPChainArmor
                {
                    if (damageModifier == 1f || damageModifier > 0.5f)
                        damageModifier = 0.5f;
                }
                else if (i.Type == armorType[2]) //PvPPlateArmor
                {
                    if (damageModifier == 1f || damageModifier > 0.25f)
                        damageModifier = 0.25f;
                }
            }

            Players.TryGetPlayer(attacker, out Players.Player attackerPl);
            Players.TakeHit(attacked, damage * damageModifier, attackerPl, ModLoader.OnHitData.EHitSourceType.PlayerClick);

#if DEBUG
            if (attackerPl != null)
                Chatting.Chat.SendToConnected(attackerPl.Name + " deals " + damage * damageModifier + " damage to " + attacked.Name);
            else
                Chatting.Chat.SendToConnected(attacked.Name + " receives " + damage * damageModifier + " damage");
#endif

            if (attacked.Health <= 0 && attackerPl != null)
            {
                Chatting.Chat.SendToConnected(attackerPl.Name + " has killed " + attacked.Name);
            }

            //Reset PvP cooldown
            PvPManagement.ResetPvPCoolDown(attacked.ID);
            if(attackerPl != null)
                PvPManagement.ResetPvPCoolDown(attackerPl.ID);

#if DEBUG
                Chatting.Chat.SendToConnected("Reset PvP cooldown.");
#endif
        }

        /*
         * IF player makes LEFT click & consumes ammo (slingbullet, bronzearrow, crossbowbolt, leadbullet)
         * THEN generate projectile
         * OTHERWISE CHECK PUNCH
         */

        //READ: https://discord.com/channels/345192439323033601/345214873082527756/835467167184977960
        /*
            var arrowType = ItemTypes.GetType("projectile_arrow");
            
            var arrowMesh = MeshedObjectType.Register(new MeshedObjectTypeSettings("CUSTOM NAME", arrowType.Mesh.MeshPath, ItemTypesServer.Texture_Water_Normal));

            var arrowClient = new ClientMeshedObject(arrowMesh);

            arrowClient.SendMoveToInterpolated(...);
            arrowClient.SendRemoval(...);
            */

        /*
            for weapons like the builtin ones, you'd have to make custom versions with the ControlledMeshes code to render them client-side

             MeshedObjects.ClientMeshedObject ¿SendRemoval?
         */
    }
}
