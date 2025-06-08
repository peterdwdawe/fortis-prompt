using LiteNetLib;
using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public struct CustomMessage : IStandardNetworkMessage<CustomMessage>
    {
        public StandardMessageType MsgType => StandardMessageType.CustomMessage;

        public int playerID;
        public string msg;

        public CustomMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public CustomMessage(int playerID, string msg)
        {
            this.playerID = playerID;
            this.msg = msg;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            playerID = reader.GetInt();
            msg = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(playerID);
            writer.Put(msg);
        }

        public bool Equals(CustomMessage other)
            => MsgType == other.MsgType
            && msg == other.msg;
    }
}
