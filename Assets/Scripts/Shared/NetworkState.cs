namespace Shared.Configuration
{
    //TODO: it's making less sense now for config to be accessed through networkState
    //TODO: maybe networkstate should be part of NetwrokManager
    public class NetworkState
    {
        public readonly NetworkConfig config;

        public NetworkState(NetworkConfig config)
        {
            this.config = config;
        }

        public float CurrentTime => CurrentTick * config.TickInterval;
        public uint CurrentTick { get; private set; } = 0;

        public void Tick()
        {
            CurrentTick++;
        }

        public void SetCurrentTick(uint currentTick)
        {
            CurrentTick = currentTick;
        }
    }
}