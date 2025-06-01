using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public struct DeathMessage : INetworkMessage<DeathMessage>
    {
        public MessageType MsgType => MessageType.Death;

        int playerID;

        public DeathMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public DeathMessage(int playerID)
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
            writer.Put((int)MsgType);
            writer.Put(playerID);
        }
    }
}
