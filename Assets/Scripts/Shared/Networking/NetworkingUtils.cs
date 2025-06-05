using LiteNetLib.Utils;
using Shared.Networking.Messages;
using System.Numerics;

namespace Shared.Networking
{
    public static partial class NetworkingUtils
    {
        public static bool TryReadNetworkMessage(this NetDataReader reader, out INetworkMessage msg)
        {
            MessageType msgType = (MessageType)reader.PeekByte();

            switch (msgType)
            {
                case MessageType.PlayerUpdate:
                    msg = new PlayerUpdateMessage(reader);
                    return true;

                case MessageType.ProjectileSpawn:
                    msg = new ProjectileSpawnMessage(reader);
                    return true;

                case MessageType.ProjectileDespawn:
                    msg = new ProjectileDespawnMessage(reader);
                    return true;

                case MessageType.HealthUpdate:
                    msg = new HealthUpdateMessage(reader);
                    return true;

                case MessageType.PlayerDeath:
                    msg = new PlayerDeathMessage(reader);
                    return true;

                case MessageType.PlayerSpawn:
                    msg = new PlayerSpawnMessage(reader);
                    return true;

                case MessageType.CustomMessage:
                    msg = new CustomMessage(reader);
                    return true;
                case MessageType.PlayerRegistration:
                    msg = new PlayerRegistrationMessage(reader);
                    return true;
                case MessageType.PlayerDeregistration:
                    msg = new PlayerDeregistrationMessage(reader);
                    return true;
                case MessageType.RequestProjectileSpawn:
                    msg = new RequestProjectileSpawnMessage(reader);
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
