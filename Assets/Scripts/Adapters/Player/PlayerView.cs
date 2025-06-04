using Shared.Player;
using System;
using UnityEngine;

namespace Adapters.Character
{
    public class PlayerView : MonoBehaviour
    {
        public static event Action<PlayerView> PlayerInstantiated;
        public event Action PlayerSpawned;
        public event Action PlayerDied;
        public event Action PlayerDestroyed;

        [SerializeField] Material localPlayerMaterial;
        [SerializeField] Material networkedPlayerMaterial;
        [SerializeField] Renderer mainCapsule;
        Renderer[] allRenderers;

        GameObject root;

        public IPlayer player { get; private set; }

        public void Setup(IPlayer player, GameObject root)
        {
            this.player = player;
            this.root = root;

            player.Died += Die;
            player.Spawned += Spawn;
            player.Destroyed += Destroy;

            mainCapsule.material = player.LocalPlayer? localPlayerMaterial : networkedPlayerMaterial;
            allRenderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in allRenderers)
            {
                renderer.enabled = false;
            }
            PlayerInstantiated?.Invoke(this);
        }
        void Destroy(IPlayer player)
        {
            PlayerDestroyed?.Invoke();

            player.Died -= Die;
            player.Spawned -= Spawn;
            player.Destroyed -= Destroy;
            this.player = null;

            Destroy(root);
        }
        void Spawn(IPlayer player)
        {
            PlayerSpawned?.Invoke();
            enabled = true;
            foreach (Renderer renderer in allRenderers)
            {
                renderer.enabled = true;
            }
        }
        void Die(IPlayer player)
        {
            Debug.Log("PlayerDied!");
            PlayerDied?.Invoke();
            enabled = false;
            foreach (Renderer renderer in allRenderers)
            {
                renderer.enabled = false;
            }
        }

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, player.Position.ToUnityVector(), 0.8f);
            transform.rotation = Quaternion.Slerp(transform.rotation, player.Rotation.ToUnityQuaternion(), 0.8f);
        }
    }
}
