using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public struct HealthUpdateMessage : INetworkMessage<HealthUpdateMessage>
    {
        public MessageType MsgType => MessageType.HealthUpdate;

        int playerID;
        byte hp;

        public HealthUpdateMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public HealthUpdateMessage(int playerID, byte hp)
        {
            this.playerID = playerID;
            this.hp = hp;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            playerID = reader.GetInt();
            hp = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(playerID);
            writer.Put(hp);
        }
    }
}
