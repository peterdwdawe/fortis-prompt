using LiteNetLib;
using LiteNetLib.Utils;
using System.Numerics;

namespace Shared.Networking.Messages
{
    public struct PlayerRegistrationMessage : IStandardNetworkMessage<PlayerRegistrationMessage>
    {
        public StandardMessageType MsgType => StandardMessageType.PlayerRegistration;

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

        public bool Equals(PlayerRegistrationMessage other)
            => MsgType == other.MsgType
            && playerID == other.playerID
            && localPlayer == other.localPlayer;
    }
}
