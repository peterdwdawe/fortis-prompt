using LiteNetLib.Utils;

namespace Shared.Configuration
{
    public class GameConfig
    {
        private readonly GameConfigData savedData;

        public GameConfigData GetData()
        {
            return savedData;
        }

        public GameConfig(GameConfigData savedData)
        {
            this.savedData = savedData;
        }

        public byte MaxPlayerCount => savedData.MaxPlayerCount;

        public float PlayerMoveSpeed => savedData.PlayerMoveSpeed;
        public float PlayerRotateSpeed => savedData.PlayerRotateSpeed;
        public float PlayerRadius => savedData.PlayerRadius;
        public int PlayerMaxHP => savedData.PlayerMaxHP;
        public float PlayerRespawnTime => savedData.PlayerRespawnTime;

        public int ProjectileDamage => savedData.ProjectileDamage;
        public float ProjectileSpeed => savedData.ProjectileSpeed;
        public float ProjectileLifetime => savedData.ProjectileLifetime;
    }

    public class GameConfigData : INetSerializable, System.IEquatable<GameConfigData>
    {
        //Max # of clients allowed to connect
        public byte MaxPlayerCount { get; set; } = 16;

        //Player speed in m/s
        public float PlayerMoveSpeed { get; set; } = 4f;

        //Player rotation speed in rotations/second
        public float PlayerRotateSpeed { get; set; } = 2f;

        //Player capsule radius in metres
        public float PlayerRadius { get; set; } = 0.5f;

        //Player max HP
        public int PlayerMaxHP { get; set; } = 100;

        //Player respawn time in seconds
        public float PlayerRespawnTime { get; set; } = 5f;

        //Projectile damage dealt on hit
        public int ProjectileDamage { get; set; } = 25;

        //Projectile speed in m/s
        public float ProjectileSpeed { get; set; } = 8f;

        //Projectile lifetime in seconds
        public float ProjectileLifetime { get; set; } = 4f;

        public void Deserialize(NetDataReader reader)
        {
            MaxPlayerCount = reader.GetByte();

            PlayerMoveSpeed = reader.GetFloat();
            PlayerRotateSpeed = reader.GetFloat();
            PlayerRadius = reader.GetFloat();
            PlayerMaxHP = reader.GetInt();
            PlayerRespawnTime = reader.GetFloat();

            ProjectileDamage = reader.GetInt();
            ProjectileSpeed = reader.GetFloat();
            ProjectileLifetime = reader.GetFloat();
        }

        public bool Equals(GameConfigData other)
            => MaxPlayerCount == other.MaxPlayerCount
            && PlayerMoveSpeed == other.PlayerMoveSpeed
            && PlayerRotateSpeed == other.PlayerRotateSpeed
            && PlayerRadius == other.PlayerRadius
            && PlayerMaxHP == other.PlayerMaxHP
            && PlayerRespawnTime == other.PlayerRespawnTime
            && ProjectileDamage == other.ProjectileDamage
            && ProjectileSpeed == other.ProjectileSpeed
            && ProjectileLifetime == other.ProjectileLifetime;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(MaxPlayerCount);
            writer.Put(PlayerMoveSpeed);
            writer.Put(PlayerRotateSpeed);
            writer.Put(PlayerRadius);
            writer.Put(PlayerMaxHP);
            writer.Put(PlayerRespawnTime);
            writer.Put(ProjectileDamage);
            writer.Put(ProjectileSpeed);
            writer.Put(ProjectileLifetime);
        }
    }
}