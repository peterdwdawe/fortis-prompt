namespace Shared.Networking.RpcMessages
{
    public interface IRpcRequestMessage : IRpcMessage { }

    public interface IRpcRequestMessage<TResponse> : IRpcRequestMessage
        where TResponse : IRpcResponseMessage { }

    public interface IRpcRequestMessage<TMessage, TResponse> : IRpcRequestMessage<TResponse>, System.IEquatable<TMessage>
        where TMessage : struct, IRpcRequestMessage<TMessage, TResponse>
        where TResponse : struct, IRpcResponseMessage<TResponse>, System.IEquatable<TResponse> { }
}
