using LiteNetLib.Utils;
using System.Numerics;

namespace Shared.Networking.RpcMessages
{
    public struct SpawnProjectileRpcResponseMessage : IRpcResponseMessage<SpawnProjectileRpcResponseMessage>
    {
        public RpcMessageType MsgType => RpcMessageType.SpawnProjectile;

        bool IRpcResponseMessage.Approved => approved;

        public bool approved;
        public int projectileID;
        public int ownerID;
        public Vector3 position;
        public Vector3 direction;

        public SpawnProjectileRpcResponseMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        private  SpawnProjectileRpcResponseMessage(bool approved, int projectileID, int ownerID, Vector3 position, Vector3 direction)
        {
            this.approved = approved;
            this.projectileID = projectileID;
            this.ownerID = ownerID;
            this.position = position;
            this.direction = direction;
        }

        public static SpawnProjectileRpcResponseMessage Denied(int ownerID)
            => new SpawnProjectileRpcResponseMessage(false, 0, ownerID, default, default);

        public static SpawnProjectileRpcResponseMessage Approved(int projectileID, int ownerID, Vector3 position, Vector3 direction)
            => new SpawnProjectileRpcResponseMessage(true, projectileID, ownerID, position, direction);

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            approved = reader.GetBool();
            projectileID = reader.GetInt();
            ownerID = reader.GetInt();
            position = reader.GetVector3();
            direction = reader.GetVector3();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(approved);
            writer.Put(projectileID);
            writer.Put(ownerID);
            writer.Put(position);
            writer.Put(direction);
        }

        public bool Equals(SpawnProjectileRpcResponseMessage other)
            => MsgType == other.MsgType
            && approved == other.approved
            && projectileID == other.projectileID
            && ownerID == other.ownerID
            && position == other.position
            && direction == other.direction;
    }
}
