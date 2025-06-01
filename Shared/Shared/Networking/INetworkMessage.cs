using LiteNetLib.Utils;

namespace Shared.Networking
{
    public enum MessageType : byte
    {
        CustomMessage = 0,
        PlayerRegistration,
        PlayerDeregistration,
        PlayerUpdate,
        ProjectileSpawn,
        ProjectileDespawn,
        HealthUpdate,
        Death,
        Respawn
    }

    public interface INetworkMessage : INetSerializable
    {
        MessageType MsgType { get; }
    }

    public interface INetworkMessage<TMessage> : INetworkMessage
        where TMessage : struct, INetworkMessage<TMessage> { }
}
