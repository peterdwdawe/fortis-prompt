namespace Client.Configuration
{
    public class ClientConfig
    {
        private readonly ClientConfigData savedData;

        public ClientConfig(ClientConfigData savedData)
        {
            this.savedData = savedData;
        }

        public float PlayerUpdateInterval => savedData.PlayerUpdateInterval;
        public string NetworkKey => savedData.NetworkKey;
        public float RpcTimeout => savedData.RpcTimeout;
    }
    public class ClientConfigData
    {
        //Time interval between transform & controls updates, in seconds
        public float PlayerUpdateInterval { get; set; } = 0.05f;

        //Must match ServerConfigData.NetworkKey to connect successfully
        public string NetworkKey { get; set; } = "fortis_connect_test";

        //duration, in seconds, to wait for an rpc response before returning a failure
        public float RpcTimeout { get; set; } = 1f;
    }
}