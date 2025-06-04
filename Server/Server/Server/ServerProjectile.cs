using Shared.Projectiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    internal class ServerProjectile : Projectile
    {
        private const float Duration = 4f;
        private readonly float endTime;
        public ServerProjectile(int Id, int ownerID, Vector3 initialPosition, Vector3 direction) : base(Id, ownerID, initialPosition, direction)
        {
            endTime = NetworkState.CurrentTime + Duration;
        }

        public override void Tick()
        {
            if (NetworkState.CurrentTime > endTime)
            {
                Expired = true;
                //don't need to return - that's taken care of in base.Tick()
            }

            base.Tick();
        }
    }
}
