namespace Shared.Configuration
{
    public class NetworkConfig
    {
        //public float TickInterval { get; private set; } = 0.02f;
        //public int TickIntervalMS() => (int)(TickInterval * 1000);
        //public float RpcTimeout { get; private set; } = 1f;
        //public byte MaxPlayers { get; private set; } = 16;
        //public float PlayerUpdateInterval { get; private set; } = 0.25f;
        //public string TestNetworkKey { get; private set; } = "fortis_connect_test";

        private readonly NetworkConfigData savedData;

        public NetworkConfig(NetworkConfigData savedData)
        {
            this.savedData = savedData;
        }

        public float TickInterval => savedData.TickInterval;
        public int TickIntervalMS() => (int)(savedData.TickInterval * 1000);

        public float RpcTimeout => savedData.RpcTimeout;

        public byte MaxPlayers => savedData.MaxPlayers;

        public float PlayerUpdateInterval => savedData.PlayerUpdateInterval;

        public string TestNetworkKey => savedData.TestNetworkKey;
    }

    public class NetworkConfigData
    {
        public float TickInterval { get; set; } = 0.01f;

        public float RpcTimeout { get; set; } = 1f;

        public byte MaxPlayers { get; set; } = 16;

        public float PlayerUpdateInterval { get; set; } = 0.02f;

        public string TestNetworkKey { get; set; } = "fortis_connect_test";
    }
}