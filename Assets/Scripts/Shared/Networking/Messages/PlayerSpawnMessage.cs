using LiteNetLib;
using LiteNetLib.Utils;
using System.Numerics;

namespace Shared.Networking.Messages
{
    public struct PlayerSpawnMessage : INetworkMessage<PlayerSpawnMessage>
    {
        public MessageType MsgType => MessageType.PlayerSpawn;

        public int playerID;
        public Vector3 position;
        public Quaternion rotation;

        public PlayerSpawnMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public PlayerSpawnMessage(int playerID, Vector3 position, Quaternion rotation)
        {
            this.playerID = playerID;
            this.position = position;
            this.rotation = rotation;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            playerID = reader.GetInt();
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

        public void Receive(NetPeer peer)
        {
            Received?.Invoke(peer, this);
        }

        internal static event System.Action<NetPeer, PlayerSpawnMessage> Received;
    }
}
