using LiteNetLib;
using LiteNetLib.Utils;
using System.Numerics;

namespace Shared.Networking.Messages
{
    public struct PlayerDeregistrationMessage : INetworkMessage<PlayerDeregistrationMessage>
    {
        public MessageType MsgType => MessageType.PlayerDeregistration;

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

        public void Receive(NetPeer peer)
        {
            Received?.Invoke(peer, this);
        }

        internal static event System.Action<NetPeer, PlayerDeregistrationMessage> Received;
    }
}
