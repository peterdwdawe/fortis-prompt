using Shared.Projectiles;
using UnityEngine;

namespace Client.Adapters.Projectiles
{
    public class ProjectileView : MonoBehaviour
    {
        private IProjectile _projectile;

        public void Setup(IProjectile player)
        {
            _projectile = player;
            _projectile.Destroyed += (_) => Destroy(gameObject);
            enabled = true;
        }

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, _projectile.Position.ToUnityVector(), 0.8f);
        }
    }
}
