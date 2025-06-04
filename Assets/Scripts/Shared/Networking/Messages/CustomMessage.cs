using LiteNetLib;
using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public struct CustomMessage : INetworkMessage<CustomMessage>
    {
        public MessageType MsgType => MessageType.CustomMessage;

        public string msg;

        public CustomMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public CustomMessage(string msg)
        {
            this.msg = msg;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            msg = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(msg);
        }

        public void Receive(NetPeer peer)
        {
            Received?.Invoke(peer, this);
        }

        internal static event System.Action<NetPeer, CustomMessage> Received;
    }
}
