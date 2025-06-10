using LiteNetLib.Utils;
using System.Numerics;

namespace Shared.Networking.RpcMessages
{
    public struct SpawnProjectileRpcRequestMessage : IRpcRequestMessage<SpawnProjectileRpcRequestMessage, SpawnProjectileRpcResponseMessage>
    {
        public RpcMessageType MsgType => RpcMessageType.SpawnProjectile;

        public int ownerID;
        public Vector3 position;
        public Vector3 direction;

        public SpawnProjectileRpcRequestMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public SpawnProjectileRpcRequestMessage(int ownerID, Vector3 position, Vector3 direction)
        {
            this.ownerID = ownerID;
            this.position = position;
            this.direction = direction;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            ownerID = reader.GetInt();
            position = reader.GetVector3();
            direction = reader.GetVector3();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(ownerID);
            writer.Put(position);
            writer.Put(direction);
        }

        public bool Equals(SpawnProjectileRpcRequestMessage other)
            => MsgType == other.MsgType
            && ownerID == other.ownerID
            && position == other.position
            && direction == other.direction;
    }
}
