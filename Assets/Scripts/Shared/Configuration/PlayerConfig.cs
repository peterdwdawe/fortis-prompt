namespace Shared.Configuration
{
    public class PlayerConfig
    {
        //public float MovementSpeed { get; private set; } = 4f;
        //public float RotationSpeed { get; private set; } = 0.25f;
        //public float Radius { get; private set; } = 0.5f;
        //public int MaxHP { get; private set; } = 100;
        //public float RespawnTime { get; private set; } = 5f;
        private readonly PlayerConfigData savedData;

        public PlayerConfig(PlayerConfigData savedData)
        {
            this.savedData = savedData;
        }

        public float MovementSpeed => savedData.MovementSpeed;
        public float RotationSpeed => savedData.RotationSpeed;
        public float Radius => savedData.Radius;
        public int MaxHP => savedData.MaxHP;
        public float RespawnTime => savedData.RespawnTime;
    }

    public class PlayerConfigData
    {
        public float MovementSpeed { get; set; } = 4f;
        public float RotationSpeed { get; set; } = 2f;
        public float Radius { get; set; } = 0.5f;
        public int MaxHP { get; set; } = 100;
        public float RespawnTime { get; set; } = 5f;
    }
}