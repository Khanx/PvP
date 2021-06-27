using Shared;
using System.Collections.Generic;
using UnityEngine;

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


    [ModLoader.ModManager]
    public static class PlayerVSPlayer
    {
        //Cooldown between shots:

        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnPlayerClicked, "Khanx.PvP.OnPlayerClicked")]
        public static void OnPlayerClicked(Players.Player player, PlayerClickedData data)
        {
            if (data.ClickType != PlayerClickedData.EClickType.Left)
                return;

            if (data.IsConsumed && data.ConsumedType == PlayerClickedData.EConsumedType.UsedAsTool)
                TryShoot(player, data);
            else
                TryPunch(player, data);
        }


        //READ: https://discord.com/channels/345192439323033601/345214873082527756/835464495744417812
                                                            //PUNCH, SLING, BOW, CROSSB, MATCHLOCK
        public static readonly long[] TimeBetweenAttacks = new long[(int)Weapon.MAX];// { 500L, 1000L, 1500L, 2500L, 3000L };

        //REAL DAMAGE
        //public static readonly float[] AttackDamage = { 35f, 50f, 100f, 300f, 500f };
        public static readonly float[] AttackDamage = new float[(int)Weapon.MAX];


        [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterWorldLoad, "Khanx.PvP.AfterWorldLoad")]
        public static void Initialize()
        {
            TimeBetweenAttacks[(int)Weapon.Punch] = 500L;

            TimeBetweenAttacks[(int)Weapon.BronzeSword]     = 500L;
            TimeBetweenAttacks[(int)Weapon.IronSword]       = 500L;
            TimeBetweenAttacks[(int)Weapon.SteelSword]      = 500L;

            TimeBetweenAttacks[(int)Weapon.BronzeSpear]     = 500L;
            TimeBetweenAttacks[(int)Weapon.IronSpear]       = 500L;
            TimeBetweenAttacks[(int)Weapon.SteelSpear]      = 500L;

            TimeBetweenAttacks[(int)Weapon.BronzeMace]      = 1000L;
            TimeBetweenAttacks[(int)Weapon.IronMace]        = 1000L;
            TimeBetweenAttacks[(int)Weapon.SteelMace]       = 1000L;

            TimeBetweenAttacks[(int)Weapon.Sling]           = 1000L;
            TimeBetweenAttacks[(int)Weapon.Bow]             = 1500L;
            TimeBetweenAttacks[(int)Weapon.Crossbow]        = 2500;
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
        }

        public static Dictionary<(Players.Player, Weapon), ServerTimeStamp> LastShoot = new Dictionary<(Players.Player, Weapon), ServerTimeStamp>();

        public static void TryShoot(Players.Player player, PlayerClickedData data)
        {
            //Chatting.Chat.SendToConnected("Player Tries to shoot");

            if (data.TypeSelected == BlockTypes.BuiltinBlocks.Indices.sling)
            {
                //TIME BETWEEM shoots
                if (LastShoot.TryGetValue((player, Weapon.Sling), out ServerTimeStamp value) && value.TimeSinceThis < TimeBetweenAttacks[(int)Weapon.Sling])
                    return;

                LastShoot[(player, Weapon.Sling)] = ServerTimeStamp.Now;

                projectiles.Add(new Projectile(ProjectileType.Sling, player.Position + Vector3.up, data.PlayerAimDirection, player.ID));
                //Chatting.Chat.SendToConnected("Sling shoot: " + player.Position);
            }
            else if (data.TypeSelected == BlockTypes.BuiltinBlocks.Indices.bow)
            {
                //TIME BETWEEM shoots
                if (LastShoot.TryGetValue((player, Weapon.Bow), out ServerTimeStamp value) && value.TimeSinceThis < TimeBetweenAttacks[(int)Weapon.Bow])
                    return;

                LastShoot[(player, Weapon.Bow)] = ServerTimeStamp.Now;

                projectiles.Add(new Projectile(ProjectileType.Arrow, player.Position + Vector3.up, data.PlayerAimDirection, player.ID));
                //Chatting.Chat.SendToConnected("Bow shoot: " + player.Position);
            }
            else if (data.TypeSelected == BlockTypes.BuiltinBlocks.Indices.crossbow)
            {
                //TIME BETWEEM shoots
                if (LastShoot.TryGetValue((player, Weapon.Crossbow), out ServerTimeStamp value) && value.TimeSinceThis < TimeBetweenAttacks[(int)Weapon.Crossbow])
                    return;

                LastShoot[(player, Weapon.Crossbow)] = ServerTimeStamp.Now;

                projectiles.Add(new Projectile(ProjectileType.Crossbow, player.Position + Vector3.up, data.PlayerAimDirection, player.ID));
                //Chatting.Chat.SendToConnected("Crossbow shoot: " + player.Position);
            }
            else if (data.TypeSelected == BlockTypes.BuiltinBlocks.Indices.matchlockgun)
            {
                //TIME BETWEEM shoots
                if (LastShoot.TryGetValue((player, Weapon.Matchlockgun), out ServerTimeStamp value) && value.TimeSinceThis < TimeBetweenAttacks[(int)Weapon.Matchlockgun])
                    return;

                LastShoot[(player, Weapon.Matchlockgun)] = ServerTimeStamp.Now;

                projectiles.Add(new Projectile(ProjectileType.Matchlock, player.Position + Vector3.up, data.PlayerAimDirection, player.ID));
                //Chatting.Chat.SendToConnected("Matchlockgun shoot: " + player.Position);
            }
        }

        public static void TryPunch(Players.Player player, PlayerClickedData data)
        {
            Weapon weapon = Weapon.Punch;

            switch(data.TypeSelected)
            {
                //Bronze Sword
                case 0: weapon = Weapon.Punch; break;
                //Iron Sword
                case 1: weapon = Weapon.Punch; break;
                //Steel Sword
                case 2: weapon = Weapon.Punch; break;
                //Bronze Spear
                case 3: weapon = Weapon.Punch; break;
                //Iron Spear
                case 4: weapon = Weapon.Punch; break;
                //Steel Spear
                case 5: weapon = Weapon.Punch; break;
                //Bonze Mace
                case 6: weapon = Weapon.Punch; break;
                //Iron Mace
                case 7: weapon = Weapon.Punch; break;
                //Steel Mace
                case 8: weapon = Weapon.Punch; break;
            }

            //TIME BETWEEM shoots
            if (LastShoot.TryGetValue((player, weapon), out ServerTimeStamp value) && value.TimeSinceThis < TimeBetweenAttacks[(int)weapon])
                return;

            LastShoot[(player, weapon)] = ServerTimeStamp.Now;

            Ray ray = new Ray(data.PlayerEyePosition, data.PlayerAimDirection);
            //ServerManager.SendParticleTrail(data.PlayerEyePosition + data.PlayerAimDirection * 3, data.PlayerEyePosition - data.PlayerAimDirection * 3, 5);

            for (int i = 0; i < Players.CountConnected; i++)
            {
                Players.Player pl = Players.GetConnectedByIndex(i);

                //A plater cannot hit himself
                if (pl.Equals(player))
                    continue;

                if (Vector3.Distance(pl.Position, player.Position) > 2)
                    continue;

                Bounds playerBounds = new Bounds(pl.Position + new Vector3(0, 0.5f, 0), new Vector3(1.25f, 2.25f, 0.5f));

                if (playerBounds.IntersectRay(ray))
                {
                    //Chatting.Chat.SendToConnected(player.Name + " hits " + pl.Name);
                    AttackPlayer(pl, AttackDamage[(int)Weapon.Punch]);
                    break;
                }
            }
        }

        public static List<Projectile> projectiles = new List<Projectile>();
        private static ServerTimeStamp nextUpdate;
        private static readonly long timeBetweenUpdates = 200L;

        //This method detects projectile collision
        [ModLoader.ModCallback(ModLoader.EModCallbackType.OnUpdate, "Khanx.PvP.OnUpdate")]
        public static void OnUpdate()
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

                //ServerManager.SendParticleTrail(projectile.lastPosition, nextPosition,  5);

                //Check if HITS a type (block)
                //Zun recommends: if (!VoxelPhysics.CanSee(position, nextPosition)) { ... hit something ... }
                if (!VoxelPhysics.CanSee(projectile.lastPosition, nextPosition))
                {
                    //Chatting.Chat.SendToConnected("Hits SOMETHING");
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
                        //Chatting.Chat.SendToConnected("Hits Zombie: " + monster.Position);
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
                        //Chatting.Chat.SendToConnected("Hits NPC: " + npc.Position);
                        projectiles.RemoveAt(i);
                        continue;
                    }
                }

                //Check if HITS a player
                for (int j = 0; j < Players.CountConnected; j++)
                {
                    Players.Player pl = Players.GetConnectedByIndex(j);

                    //A player cannot shoot himself
                    if (pl.ID.Equals(projectile.shooter))
                        continue;

                    if (Vector3.Distance(pl.Position, nextPosition) > (2 + distanceBetweenShots))
                        continue;

                    Bounds playerBounds = new Bounds(pl.Position + new Vector3(0, 0.5f, 0), new Vector3(1.25f, 2.25f, 0.5f));

                    if (playerBounds.IntersectRay(ray))
                    {
                        //Chatting.Chat.SendToConnected(Players.GetPlayer(projectile.shooter).Name + " shoots " + pl.Name);
                        
                        switch(projectile.projectileType)
                        {
                            case ProjectileType.Sling:      AttackPlayer(pl, AttackDamage[(int)Weapon.Sling]);          break;
                            case ProjectileType.Arrow:      AttackPlayer(pl, AttackDamage[(int)Weapon.Bow]);            break;
                            case ProjectileType.Crossbow:   AttackPlayer(pl, AttackDamage[(int)Weapon.Crossbow]);       break;
                            case ProjectileType.Matchlock:  AttackPlayer(pl, AttackDamage[(int)Weapon.Matchlockgun]);   break;
                        }
                        break;
                    }
                }

                projectile.lastPosition = nextPosition;
                projectiles[i] = projectile;
            }
        }

        public static void AttackPlayer(Players.Player attacked, float damage)
        {
            float damageModifier = 1;

            foreach(var i in attacked.Inventory.Items)
            {
                switch (i.Type)
                {
                    //Light armor
                    case 0:
                        //The if works to use the BEST armor in the inventory
                        if(damageModifier == 1)
                            damageModifier = 0.25f;
                    break;

                    //Medium armor
                    case 1:
                        if (damageModifier == 1 || damageModifier <0.5f)
                            damageModifier = 0.50f;
                    break;

                    //Heavy armor
                    case 2:
                        if (damageModifier == 1 || damageModifier < 0.75f)
                            damageModifier = 0.75f;
                    break;
                }
            }

            Players.TakeHit(attacked, damage * damageModifier);
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
