using LiteNetLib.Utils;
using System.Numerics;

namespace Shared.Networking.Messages
{
    public struct PlayerRegistrationMessage : INetworkMessage<PlayerRegistrationMessage>
    {
        TODO(); // separate message (or field in this message) for sending to a brand new client

        public MessageType MsgType => MessageType.PlayerRegistration;

        int playerID;

        public PlayerRegistrationMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public PlayerRegistrationMessage(int playerID, Vector3 position, Quaternion rotation)
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
    }
}
