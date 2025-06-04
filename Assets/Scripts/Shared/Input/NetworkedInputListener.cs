using System;
using System.Numerics;
using LiteNetLib;
using Shared.Networking.Messages;
using Shared.Player;

namespace Shared.Input
{
    public class NetworkedInputListener : IInputListener
    {
        public event Action<Vector3, Quaternion> OnTransformUpdated;

        //Currently never called by design. maybe revisit and improve
        public event Action OnShootLocal;

        //public event OnShootHandler OnShootNetworked;

        public Vector2 Movement { get; private set; }

        //public void Register()
        //{
        //    PlayerUpdateMessage.Received += PlayerUpdateMessage_Received;
        //    //ProjectileSpawnMessage.Received += ProjectileSpawnMessage_Received;
        //}

        //private void ProjectileSpawnMessage_Received(NetPeer peer, ProjectileSpawnMessage message)
        //{
        //    if (message.ownerID != ID)
        //        return;

        //    OnShootNetworked?.Invoke(message.projectileID, message.position, message.direction);
        //}

        public void Update(Vector2 input, Vector3 position, Quaternion rotation)
        {
            Movement = input;
            OnTransformUpdated?.Invoke(position, rotation);
        }

        //public void Deregister() 
        //{
        //    //TODO();
        //    PlayerUpdateMessage.Received -= PlayerUpdateMessage_Received;
        //}

        public readonly int ID;

        public NetworkedInputListener(int ID)
        {
            this.ID = ID;
            //Register();
        }

        //void OnPlayerUpdateReceived(NetPeer peer, PlayerUpdateMessage message)
        //{

        //}

        //public void Cleanup()
        //{
        //    //TODO(); //ensure this is called when we're done with it
        //    Deregister();
        //}
    }
}