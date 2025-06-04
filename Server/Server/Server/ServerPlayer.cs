using Shared.Input;
using Shared.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class ServerPlayer : Player
    {
        public ServerPlayer(int id, IInputListener inputListener, bool local) : base(id, inputListener, local)
        {

        }
        float respawnTimer = 0f;

        protected override void TickAlive()
        {
            base.TickAlive();
            respawnTimer = 0f;
        }

        protected override void TickDead()
        {
            base.TickDead();
            respawnTimer += NetworkConfig.TickInterval;

            if(respawnTimer >= PlayerConfig.RespawnTime)
            {
                RequestRespawn();
            }
        }
    }
}
