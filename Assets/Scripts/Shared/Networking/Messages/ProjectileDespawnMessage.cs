using LiteNetLib;
using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public struct ProjectileDespawnMessage : INetworkMessage<ProjectileDespawnMessage>
    {
        public MessageType MsgType => MessageType.ProjectileDespawn;

        public int projectileID;

        public ProjectileDespawnMessage(int projectileID)
        {
            this.projectileID = projectileID;
        }
        public ProjectileDespawnMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            projectileID = reader.GetInt();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(projectileID);
        }

        public void Receive(NetPeer peer)
        {
            Received?.Invoke(peer, this);
        }

        internal static event System.Action<NetPeer, ProjectileDespawnMessage> Received;
    }
}
