namespace Shared.Configuration
{
    public class ProjectileConfig
    {
        //public int Damage { get; private set; } = 25;
        //public float MovementSpeed { get; private set; } = 8f;
        //public float Duration { get; private set; } = 4f;
        private readonly ProjectileConfigData savedData;

        public ProjectileConfig(ProjectileConfigData savedData)
        {
            this.savedData = savedData;
        }

        public int Damage => savedData.Damage;
        public float MovementSpeed => savedData.MovementSpeed;
        public float Duration => savedData.Duration;
    }

    public class ProjectileConfigData
    {
        public int Damage { get; set; } = 25;
        public float MovementSpeed { get; set; } = 8f;
        public float Duration { get; set; } = 4f;
    }
}