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

        public Weapon weapon;

        public Vector3 startPostion;
        public Vector3 velocity;
        public Vector3 lastPosition;
        public ServerTimeStamp shootTimeMS;
        public Players.PlayerIDShort shooter;

        public Projectile(Weapon weapon, Vector3 startPostion, Vector3 direction, Players.PlayerIDShort shooter)
        {
            this.weapon = weapon;
            this.startPostion = startPostion;
            this.lastPosition = startPostion;
            this.shootTimeMS = ServerTimeStamp.Now;
            this.shooter = shooter;

            switch (weapon)
            {
                case Weapon.Sling: this.velocity = direction * SlingerForce; break;
                case Weapon.Bow: this.velocity = direction * BowForce; break;
                case Weapon.Crossbow: this.velocity = direction * CrossbowForce; break;
                case Weapon.Musket: this.velocity = direction * MatchlockForce; break;
                default: this.velocity = Vector3.zero; break;
            }
        }
    }
}
