using LiteNetLib.Utils;
using Shared.Networking.Messages;
using Shared.Networking.RPC;
using System.Numerics;

namespace Shared.Networking
{
    public static class NetworkingExtensions
    {
        public static bool TryReadStandardNetworkMessage(this NetDataReader reader, out IStandardNetworkMessage msg)
        {
            StandardMessageType msgType = (StandardMessageType)reader.PeekByte();

            switch (msgType)
            {
                case StandardMessageType.GameConfiguration:
                    msg = new GameConfigurationMessage(reader);
                    return true;

                case StandardMessageType.PlayerUpdate:
                    msg = new PlayerUpdateMessage(reader);
                    return true;

                case StandardMessageType.ProjectileSpawn:
                    msg = new ProjectileSpawnMessage(reader);
                    return true;

                case StandardMessageType.ProjectileDespawn:
                    msg = new ProjectileDespawnMessage(reader);
                    return true;

                case StandardMessageType.PlayerHPUpdate:
                    msg = new PlayerHPUpdateMessage(reader);
                    return true;

                case StandardMessageType.PlayerDeath:
                    msg = new PlayerDeathMessage(reader);
                    return true;

                case StandardMessageType.PlayerSpawn:
                    msg = new PlayerSpawnMessage(reader);
                    return true;

                case StandardMessageType.CustomMessage:
                    msg = new CustomMessage(reader);
                    return true;

                case StandardMessageType.PlayerRegistration:
                    msg = new PlayerRegistrationMessage(reader);
                    return true;

                case StandardMessageType.PlayerDeregistration:
                    msg = new PlayerDeregistrationMessage(reader);
                    return true;

                //case MessageType.RequestProjectileSpawn:
                //    msg = new RequestProjectileSpawnMessage(reader);
                //    return true;

                default:
                    msg = default;
                    return false;
            }
        }

        public static bool TryReadRpcRequestMessage(this NetDataReader reader, out IRpcRequestMessage msg)
        {
            RpcMessageType msgType = (RpcMessageType)reader.PeekByte();

            switch (msgType)
            {
                case RpcMessageType.SpawnProjectile:
                    msg = new SpawnProjectileRpcRequestMessage(reader);
                    return true;

                default:
                    msg = default;
                    return false;
            }
        }
        public static bool TryReadRpcResponseMessage(this NetDataReader reader, out IRpcResponseMessage msg)
        {
            RpcMessageType msgType = (RpcMessageType)reader.PeekByte();

            switch (msgType)
            {
                case RpcMessageType.SpawnProjectile:
                    msg = new SpawnProjectileRpcResponseMessage(reader);
                    return true;

                default:
                    msg = default;
                    return false;
            }
        }

        

        public static Vector2 GetVector2(this NetDataReader reader)
        {
            return new Vector2(reader.GetFloat(), reader.GetFloat());
        }
        public static Vector3 GetVector3(this NetDataReader reader)
        {
            return new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }
        public static Vector4 GetVector4(this NetDataReader reader)
        {
            return new Vector4(reader.GetFloat(), reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }
        public static Quaternion GetQuaternion(this NetDataReader reader)
        {
            return new Quaternion(reader.GetFloat(), reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
        }

        public static void Put(this NetDataWriter writer, Vector2 v)
        {
            writer.Put(v.X);
            writer.Put(v.Y);
        }
        public static void Put(this NetDataWriter writer, Vector3 v)
        {
            writer.Put(v.X);
            writer.Put(v.Y);
            writer.Put(v.Z);
        }
        public static void Put(this NetDataWriter writer, Vector4 v)
        {
            writer.Put(v.X);
            writer.Put(v.Y);
            writer.Put(v.Z);
            writer.Put(v.W);
        }
        public static void Put(this NetDataWriter writer, Quaternion q)
        {
            writer.Put(q.X);
            writer.Put(q.Y);
            writer.Put(q.Z);
            writer.Put(q.W);
        }
    }
}
