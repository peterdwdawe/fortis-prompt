using LiteNetLib;
using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public struct PlayerHPUpdateMessage : IStandardNetworkMessage<PlayerHPUpdateMessage>
    {
        public StandardMessageType MsgType => StandardMessageType.PlayerHPUpdate;

        public int playerID;
        public int hp;

        public PlayerHPUpdateMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public PlayerHPUpdateMessage(int playerID, int hp)
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

        public bool Equals(PlayerHPUpdateMessage other)
            => MsgType == other.MsgType
            && playerID == other.playerID
            && hp == other.hp;
    }
}
