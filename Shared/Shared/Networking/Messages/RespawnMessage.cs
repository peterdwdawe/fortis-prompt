using LiteNetLib.Utils;
using System.Numerics;

namespace Shared.Networking.Messages
{
    public struct RespawnMessage : INetworkMessage<RespawnMessage>
    {
        public MessageType MsgType => MessageType.Respawn;

        int playerID;
        Vector3 position;
        Quaternion rotation;

        public RespawnMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public RespawnMessage(int playerID, Vector3 position, Quaternion rotation)
        {
            this.playerID = playerID;
            this.position = position;
            this.rotation = rotation;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            playerID = reader.GetInt();
            position = reader.GetVector3();
            rotation = reader.GetQuaternion();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(playerID);
            writer.Put(position);
            writer.Put(rotation);
        }
    }
}
