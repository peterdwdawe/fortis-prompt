namespace Shared.Configuration
{
    public class NetworkConfig
    {

        public float TickInterval { get; private set; } = 0.02f;
        public int TickIntervalMS() => (int)(TickInterval * 1000);

        public byte MaxPlayers { get; private set; } = 16;

        public ushort PlayerUpdateTickCount { get; private set; } = 4;

        public string TestNetworkKey { get; private set; } = "fortis_connect_test";
    }
}