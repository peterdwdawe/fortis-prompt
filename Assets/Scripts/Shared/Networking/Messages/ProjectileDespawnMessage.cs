using LiteNetLib;
using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public struct ProjectileDespawnMessage : IStandardNetworkMessage<ProjectileDespawnMessage>
    {
        public StandardMessageType MsgType => StandardMessageType.ProjectileDespawn;

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

        public bool Equals(ProjectileDespawnMessage other)
            => MsgType == other.MsgType
            && projectileID == other.projectileID;
    }
}
