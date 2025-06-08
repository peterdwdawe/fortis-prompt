using LiteNetLib;
using LiteNetLib.Utils;

namespace Shared.Networking.RPC
{
    public enum RpcMessageType : byte
    {
        SpawnProjectile = 0,
    }

    public interface IRpcMessage : INetSerializable
    {
        RpcMessageType MsgType { get; }
    }
    public interface IRpcRequestMessage : IRpcMessage
    {

    }

    public interface IRpcRequestMessage<TResponse> : IRpcRequestMessage
        where TResponse : IRpcResponseMessage
    {
        //is this used?
    }

    public interface IRpcRequestMessage<TMessage, TResponse> : IRpcRequestMessage<TResponse>, System.IEquatable<TMessage>
        where TMessage : struct, IRpcRequestMessage<TMessage, TResponse>
        where TResponse : struct, IRpcResponseMessage<TResponse>, System.IEquatable<TResponse>
    {

    }

    public interface IRpcResponseMessage : IRpcMessage
    {

    }

    public interface IRpcResponseMessage<TMessage> : IRpcResponseMessage, System.IEquatable<TMessage>
        where TMessage : struct, IRpcResponseMessage<TMessage>
    {

    }
}
