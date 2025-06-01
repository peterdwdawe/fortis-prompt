using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public struct ProjectileDespawnMessage : INetworkMessage<ProjectileDespawnMessage>
    {
        public MessageType MsgType => MessageType.ProjectileDespawn;

        ushort projectileID;

        public ProjectileDespawnMessage(ushort projectileID)
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
            projectileID = reader.GetUShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(projectileID);
        }
    }
}
