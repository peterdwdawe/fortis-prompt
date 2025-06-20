﻿namespace Shared.Networking.RpcMessages
{
    public interface IRpcResponseMessage : IRpcMessage
    {
        bool Approved { get; }
    }

    public interface IRpcResponseMessage<TMessage> : IRpcResponseMessage, System.IEquatable<TMessage>
        where TMessage : struct, IRpcResponseMessage<TMessage> { }
}
