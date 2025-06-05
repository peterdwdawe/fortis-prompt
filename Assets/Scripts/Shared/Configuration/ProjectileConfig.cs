namespace Shared.Configuration
{
    public class ProjectileConfig
    {
        public int Damage { get; private set; } = 25;
        public float MovementSpeed { get; private set; } = 8f;
        public float Duration { get; private set; } = 4f;
    }
}