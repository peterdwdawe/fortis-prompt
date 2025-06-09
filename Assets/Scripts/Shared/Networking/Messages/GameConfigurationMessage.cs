using LiteNetLib.Utils;
using Shared.Configuration;

namespace Shared.Networking.Messages
{
    public struct GameConfigurationMessage : IStandardNetworkMessage<GameConfigurationMessage>
    {
        public StandardMessageType MsgType => StandardMessageType.GameConfiguration;

        public GameConfigData configData;

        public GameConfigurationMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public GameConfigurationMessage(GameConfigData configData)
        {
            this.configData = configData;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            configData = new GameConfigData();
            configData.Deserialize(reader);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(configData);
        }

        public bool Equals(GameConfigurationMessage other)
            => MsgType == other.MsgType
            && configData.Equals(other.configData);
    }
}
