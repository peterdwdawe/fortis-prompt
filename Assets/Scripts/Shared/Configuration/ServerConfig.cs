namespace Shared.Configuration
{
    public class ServerConfig
    {
        //TODO(); should be in server proj and namespace
        private readonly ServerConfigData savedData;

        public ServerConfig(ServerConfigData savedData)
        {
            this.savedData = savedData;
        }
        public string NetworkKey => savedData.NetworkKey;
        public int Port => savedData.Port;
        public float TickInterval => savedData.ServerTickInterval;
        public int TickIntervalMS => (int)(savedData.ServerTickInterval * 1000);
        public float RpcTimeout => savedData.RpcTimeout;
    }
    public class ServerConfigData
    {
        //Time interval between server network updates in seconds
        public float ServerTickInterval { get; set; } = 0.015f;

        //Port number to run on
        public int Port { get; set; } = 5000;

        //Must match ClientConfigData.NetworkKey to connect successfully
        public string NetworkKey { get; set; } = "fortis_connect_test";

        //duration, in seconds, to wait for an rpc response before returning a failure
        public float RpcTimeout { get; set; } = 1f;
    }
}