using LiteNetLib;
using LiteNetLib.Utils;
using System.Numerics;

namespace Shared.Networking.Messages
{
    public struct PlayerRegistrationMessage : INetworkMessage<PlayerRegistrationMessage>
    {
        //TODO(); // separate message (or field in this message) for sending to a brand new client

        public MessageType MsgType => MessageType.PlayerRegistration;

        public int playerID;
        public bool localPlayer;

        public PlayerRegistrationMessage(NetDataReader reader) : this()
        {
            Deserialize(reader);
        }

        public PlayerRegistrationMessage(int playerID, bool localPlayer)
        {
            this.playerID = playerID;
            this.localPlayer = localPlayer;
        }

        public void Deserialize(NetDataReader reader)
        {
            reader.SkipBytes(1);    // skip msgID byte
            playerID = reader.GetInt();
            localPlayer = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put((byte)MsgType);
            writer.Put(playerID);
            writer.Put(localPlayer);
        }

        public void Receive(NetPeer peer)
        {
            Received?.Invoke(peer, this);
        }

        internal static event System.Action<NetPeer, PlayerRegistrationMessage> Received;
    }
}
