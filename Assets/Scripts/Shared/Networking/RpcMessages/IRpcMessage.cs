using LiteNetLib.Utils;

namespace Shared.Networking.RpcMessages
{
    public enum RpcMessageType : byte
    {
        SpawnProjectile = 0,
    }

    public interface IRpcMessage : INetSerializable
    {
        RpcMessageType MsgType { get; }
    }
}
