using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public struct PlayerDeregistrationMessage : IStandardNetworkMessage<PlayerDeregistrationMessage>
    {
        public StandardMessageType MsgType => StandardMessageType.PlayerDeregistration;

        public int playerID;

        public PlayerDeregistrationMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public PlayerDeregistrationMessage(int playerID)
        {
            this.playerID = playerID;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            playerID = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(playerID);
        }

        public bool Equals(PlayerDeregistrationMessage other)
            => MsgType == other.MsgType
            && playerID == other.playerID;
    }
}
