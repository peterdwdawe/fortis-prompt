using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal static class PeerLookup
    {
        static Dictionary<int, NetPeer> peers;

        static PeerLookup()
        {
            peers = new Dictionary<int, NetPeer>(NetworkConfig.MaxConnectionCount);
        }

        internal static void OnPeerConnected(NetPeer peer)
        {
            var ID = peer.Id;
            if (peers.ContainsKey(ID))
            {
                Console.WriteLine($"Registration Error: peer ID {ID} already exists! overwriting...");
            }

            peers[ID] = peer;

            TODO();//notify registration
        }
        internal static void OnPeerDisconnected(NetPeer peer)
        {
            var ID = peer.Id;
            if (!peers.ContainsKey(ID))
            {
                Console.WriteLine($"Deregistration Error: peer ID {ID} not found in lookup! ignoring...");
                return;
            }

            peers.Remove(ID);

            TODO();//notify deregistration
        }
    }
}
