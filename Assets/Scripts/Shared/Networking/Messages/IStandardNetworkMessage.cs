using LiteNetLib;
using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public enum StandardMessageType : byte
    {
        CustomMessage = 0,
        GameConfiguration,
        PlayerRegistration,
        PlayerDeregistration,
        PlayerUpdate,
        RequestProjectileSpawn,
        ProjectileSpawn,
        ProjectileDespawn,
        PlayerHPUpdate,
        PlayerDeath,
        PlayerSpawn
    }

    public interface IStandardNetworkMessage : INetSerializable
    {
        StandardMessageType MsgType { get; }
    }

    public interface IStandardNetworkMessage<TMessage> : IStandardNetworkMessage, System.IEquatable<TMessage>
        where TMessage : struct, IStandardNetworkMessage<TMessage>
    {
    
    }
}
