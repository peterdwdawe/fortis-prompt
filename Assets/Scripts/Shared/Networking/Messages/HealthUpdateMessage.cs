using LiteNetLib;
using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public struct HealthUpdateMessage : INetworkMessage<HealthUpdateMessage>
    {
        public MessageType MsgType => MessageType.HealthUpdate;

        public int playerID;
        public int hp;

        public HealthUpdateMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public HealthUpdateMessage(int playerID, int hp)
        {
            this.playerID = playerID;
            this.hp = hp;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            playerID = reader.GetInt();
            hp = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(playerID);
            writer.Put(hp);
        }

        public void Receive(NetPeer peer)
        {
            Received?.Invoke(peer, this);
        }

        public bool Equals(HealthUpdateMessage other)
            => MsgType == other.MsgType
            && playerID == other.playerID
            && hp == other.hp;

        internal static event System.Action<NetPeer, HealthUpdateMessage> Received;
    }
}
