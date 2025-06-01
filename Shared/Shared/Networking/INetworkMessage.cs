using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Networking
{
    public static partial class NetworkingUtils
    {
        public static void Send(this NetDataWriter writer, INetworkMessage message)
        {
            writer.Put(message);
        }

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

                case MessageType.Death:
                    msg = new DeathMessage(reader);
                    return true;

                case MessageType.Respawn:
                    msg = new RespawnMessage(reader);
                    return true;

                case MessageType.CustomMessage:
                    msg = new CustomMessage(reader);
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


    public enum MessageType : byte
    {
        CustomMessage = 0,
        PlayerUpdate = 1,
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
        where TMessage : struct, INetworkMessage<TMessage>
    {
    }

    public struct CustomMessage : INetworkMessage<CustomMessage>
    {
        public MessageType MsgType => MessageType.CustomMessage;

        string msg;

        public CustomMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public CustomMessage(string msg)
        {
            this.msg = msg;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            msg = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(msg);
        }
    }
    public struct RespawnMessage : INetworkMessage<RespawnMessage>
    {
        public MessageType MsgType => MessageType.Respawn;

        byte playerID;
        Vector3 position;
        Quaternion rotation;

        public RespawnMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public RespawnMessage(byte playerID, Vector3 position, Quaternion rotation)
        {
            this.playerID = playerID;
            this.position = position;
            this.rotation = rotation;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            playerID = reader.GetByte();
            position = reader.GetVector3();
            rotation = reader.GetQuaternion();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(playerID);
            writer.Put(position);
            writer.Put(rotation);
        }
    }

    public struct DeathMessage : INetworkMessage<DeathMessage>
    {
        public MessageType MsgType => MessageType.Death;

        byte playerID;

        public DeathMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public DeathMessage(byte playerID)
        {
            this.playerID = playerID;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            playerID = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(playerID);
        }
    }

    public struct HealthUpdateMessage : INetworkMessage<HealthUpdateMessage>
    {
        public MessageType MsgType => MessageType.HealthUpdate;

        byte playerID;
        byte hp;

        public HealthUpdateMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public HealthUpdateMessage(byte playerID, byte hp)
        {
            this.playerID = playerID;
            this.hp = hp;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            playerID = reader.GetByte();
            hp = reader.GetByte();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(playerID);
            writer.Put(hp);
        }
    }

    public struct ProjectileDespawnMessage : INetworkMessage<ProjectileDespawnMessage>
    {
        public MessageType MsgType => MessageType.ProjectileDespawn;

        ushort projectileID;

        public ProjectileDespawnMessage(ushort projectileID)
        {
            this.projectileID = projectileID;
        }
        public ProjectileDespawnMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            projectileID = reader.GetUShort();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(projectileID);
        }
    }

    public struct ProjectileSpawnMessage : INetworkMessage<ProjectileSpawnMessage>
    {
        public MessageType MsgType => MessageType.ProjectileSpawn;

        ushort projectileID;
        byte ownerID;
        Vector3 position;
        Quaternion rotation;

        public ProjectileSpawnMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            projectileID = reader.GetUShort();
            ownerID = reader.GetByte();
            position = reader.GetVector3();
            rotation = reader.GetQuaternion();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(projectileID);
            writer.Put(ownerID);
            writer.Put(position);
            writer.Put(rotation);
        }
    }

    public struct PlayerUpdateMessage : INetworkMessage<PlayerUpdateMessage>
    {
        public PlayerUpdateMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public PlayerUpdateMessage(byte playerID, Vector2 input, Vector3 position, Quaternion rotation)
        {
            this.playerID = playerID;
            this.input = input;
            this.position = position;
            this.rotation = rotation;
        }

        public MessageType MsgType => MessageType.PlayerUpdate;

        byte playerID;
        Vector2 input;
        Vector3 position;
        Quaternion rotation;
        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            playerID = reader.GetByte();
            input = reader.GetVector2();
            position = reader.GetVector3();
            rotation = reader.GetQuaternion();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(playerID);
            writer.Put(input);
            writer.Put(position);
            writer.Put(rotation);
        }
    }
   
}
