using LiteNetLib;
using LiteNetLib.Utils;

namespace Shared.Networking.Messages
{
    public enum MessageType : byte
    {
        CustomMessage = 0,
        PlayerRegistration,
        PlayerDeregistration,
        PlayerUpdate,
        RequestProjectileSpawn,
        ProjectileSpawn,
        ProjectileDespawn,
        HealthUpdate,
        PlayerDeath,
        PlayerSpawn
    }

    public interface INetworkMessage : INetSerializable
    {
        MessageType MsgType { get; }

        void Receive(NetPeer peer);
    }

    public interface INetworkMessage<TMessage> : INetworkMessage, System.IEquatable<TMessage>
        where TMessage : struct, INetworkMessage<TMessage>
    {
    
    }
}
