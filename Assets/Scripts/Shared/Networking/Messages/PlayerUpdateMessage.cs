using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Shared.Networking.Messages
{

    public struct PlayerUpdateMessage : INetworkMessage<PlayerUpdateMessage>
    {
        public PlayerUpdateMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public PlayerUpdateMessage(int playerID, Vector2 input, Vector3 position, Quaternion rotation)
        {
            this.playerID = playerID;
            this.input = input;
            this.position = position;
            this.rotation = rotation;
        }

        public MessageType MsgType => MessageType.PlayerUpdate;

        public int playerID;
        public Vector2 input;
        public Vector3 position;
        public Quaternion rotation;
        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            playerID = reader.GetInt();
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

        public void Receive(NetPeer peer)
        {
            Received?.Invoke(peer, this);
        }

        public bool Equals(PlayerUpdateMessage other)
            => MsgType == other.MsgType
            && playerID == other.playerID
            && input == other.input
            && position == other.position
            && rotation == other.rotation;

        internal static event System.Action<NetPeer, PlayerUpdateMessage> Received;
    }
}
