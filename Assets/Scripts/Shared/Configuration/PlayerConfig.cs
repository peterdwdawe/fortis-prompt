namespace Shared.Configuration
{
    public class PlayerConfig
    {
        public float MovementSpeed { get; private set; } = 4f;
        public float RotationSpeed { get; private set; } = 0.25f;
        public float Radius { get; private set; } = 0.5f;
        public int MaxHP { get; private set; } = 100;
        public float RespawnTime { get; private set; } = 5f;
    }
}