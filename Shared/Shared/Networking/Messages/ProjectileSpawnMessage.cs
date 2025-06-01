using LiteNetLib.Utils;
using System.Numerics;

namespace Shared.Networking.Messages
{
    public struct ProjectileSpawnMessage : INetworkMessage<ProjectileSpawnMessage>
    {
        public MessageType MsgType => MessageType.ProjectileSpawn;

        ushort projectileID;
        byte ownerID;
        Vector3 position;
        Quaternion rotation;

        public ProjectileSpawnMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            projectileID = reader.GetUShort();
            ownerID = reader.GetByte();
            position = reader.GetVector3();
            rotation = reader.GetQuaternion();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(projectileID);
            writer.Put(ownerID);
            writer.Put(position);
            writer.Put(rotation);
        }
    }
}
