using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Projectiles
{
    //public class ProjectileLookup
    //{
    //    Dictionary<ushort, Projectile> projectiles;

    //    public ProjectileLookup()
    //    {
    //        projectiles = new Dictionary<ushort, Projectile>(NetworkConfig.MaxConnectionCount * 16);
    //    }

    //    public void OnProjectileInstantiated(Projectile projectile)
    //    {
    //        var ID = projectile.ID;
    //        if (projectiles.ContainsKey(ID))
    //        {
    //            Console.WriteLine($"OnProjectileInstantiated Error: projectile ID {ID} already exists! overwriting...");
    //            //TODO();//cleanup previous? this shouldn't be happening anyway
    //        }

    //        projectiles[ID] = projectile;
    //    }
    //    public void OnProjectileDestroyed(Projectile projectile)
    //    {
    //        var ID = projectile.ID;
    //        if (!projectiles.ContainsKey(ID))
    //        {
    //            Console.WriteLine($"OnProjectileDestroyed Error: projectile ID {ID} not found in lookup! ignoring...");
    //            return;
    //        }

    //        projectiles.Remove(ID);
    //    }
    //    public bool TryGetProjectile(ushort ID, out Projectile projectile)
    //        => projectiles.TryGetValue(ID, out projectile);
    //}
}
