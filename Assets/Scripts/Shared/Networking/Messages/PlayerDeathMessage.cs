using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public struct PlayerDeathMessage : IStandardNetworkMessage<PlayerDeathMessage>
    {
        public StandardMessageType MsgType => StandardMessageType.PlayerDeath;

        public int playerID;

        public PlayerDeathMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public PlayerDeathMessage(int playerID)
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

        public bool Equals(PlayerDeathMessage other)
            => MsgType == other.MsgType
            && playerID == other.playerID;
    }
}
