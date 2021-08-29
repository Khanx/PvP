using UnityEngine;

namespace PvP
{
    public struct Projectile
    {
        //READ: https://discord.com/channels/345192439323033601/345214873082527756/835464495744417812
        public static float SlingerForce = 20f;
        public static float BowForce = 40f;
        public static float CrossbowForce = 50f;
        public static float MatchlockForce = 90f;

        public ProjectileType projectileType;

        public Vector3 startPostion;
        public Vector3 velocity;
        public Vector3 lastPosition;
        public ServerTimeStamp shootTimeMS;
        public NetworkID shooter;

        public Projectile(ProjectileType projectileType, Vector3 startPostion, Vector3 direction, NetworkID shooter)
        {
            this.projectileType = projectileType;
            this.startPostion = startPostion;
            this.lastPosition = startPostion;
            this.shootTimeMS = ServerTimeStamp.Now;
            this.shooter = shooter;

            switch (projectileType)
            {
                case ProjectileType.Sling: this.velocity = direction * SlingerForce; break;
                case ProjectileType.Arrow: this.velocity = direction * BowForce; break;
                case ProjectileType.Crossbow: this.velocity = direction * CrossbowForce; break;
                case ProjectileType.Matchlock: this.velocity = direction * MatchlockForce; break;
                default: this.velocity = Vector3.zero; break;
            }
        }
    }
}
