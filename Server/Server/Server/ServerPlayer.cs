using Shared.Configuration;
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
        private int deadTick = 0;
        private readonly int respawnWaitTicks;

        public ServerPlayer(int id, IInputListener inputListener, bool localPlayer, PlayerConfig playerConfig, NetworkConfig networkConfig) 
            : base(id, inputListener, localPlayer, playerConfig, networkConfig)
        {
            respawnWaitTicks = (int) (playerConfig.RespawnTime / networkConfig.TickInterval);
        }

        protected override void TickAlive()
        {
            base.TickAlive();
            deadTick = 0;
        }

        protected override void TickDead()
        {
            base.TickDead();
            deadTick++;

            if (deadTick >= respawnWaitTicks)
            {
                RequestRespawn();
            }
        }
    }
}
