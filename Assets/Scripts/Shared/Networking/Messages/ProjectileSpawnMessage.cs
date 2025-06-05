using LiteNetLib;
using LiteNetLib.Utils;
using System.Numerics;

namespace Shared.Networking.Messages
{
    public struct ProjectileSpawnMessage : INetworkMessage<ProjectileSpawnMessage>
    {
        public MessageType MsgType => MessageType.ProjectileSpawn;

        public int projectileID;
        public int ownerID;
        public Vector3 position;
        public Vector3 direction;

        public ProjectileSpawnMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public ProjectileSpawnMessage(int projectileID, int ownerID, Vector3 position, Vector3 direction)
        {
            this.projectileID = projectileID;
            this.ownerID = ownerID;
            this.position = position;
            this.direction = direction;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            projectileID = reader.GetInt();
            ownerID = reader.GetInt();
            position = reader.GetVector3();
            direction = reader.GetVector3();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(projectileID);
            writer.Put(ownerID);
            writer.Put(position);
            writer.Put(direction);
        }

        public void Receive(NetPeer peer)
        {
            Received?.Invoke(peer, this);
        }

        public bool Equals(ProjectileSpawnMessage other)
            => MsgType == other.MsgType
            && projectileID == other.projectileID
            && ownerID == other.ownerID
            && position == other.position
            && direction == other.direction;

        internal static event System.Action<NetPeer, ProjectileSpawnMessage> Received;
    }
}
