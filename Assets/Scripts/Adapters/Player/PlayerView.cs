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
        [SerializeField] MeshRenderer mainCapsule;
        [SerializeField][Min(0f)] float destructionWaitTime = 2f;


        [SerializeField] ParticleSystem spawnEffect;
        [SerializeField] ParticleSystem hitEffect;
        [SerializeField] ParticleSystem deathEffect;
        [SerializeField] ParticleSystem destroyEffect;

        MeshRenderer[] allRenderers;

        GameObject root;

        public IPlayer player { get; private set; }

        public void Setup(IPlayer player, GameObject root)
        {
            this.player = player;
            this.root = root;

            player.Died += Die;
            player.Spawned += Spawn;
            player.Destroyed += Destroy;
            player.HPReduced += Hit;

            mainCapsule.material = player.LocalPlayer? localPlayerMaterial : networkedPlayerMaterial;
            allRenderers = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer renderer in allRenderers)
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
            player.HPReduced -= Hit;
            this.player = null;
            enabled = false;

            foreach (MeshRenderer renderer in allRenderers)
            {
                renderer.enabled = false;
            }
            destroyEffect.Play();

            Destroy(root, destructionWaitTime);
            Destroy(this);
        }

        private void Hit(IPlayer player)
        {
            hitEffect.Play();
        }

        void Spawn(IPlayer player)
        {
            PlayerSpawned?.Invoke();
            enabled = true;
            foreach (MeshRenderer renderer in allRenderers)
            {
                renderer.enabled = true;
            }
            spawnEffect.Play();
        }
        void Die(IPlayer player)
        {
            Debug.Log("PlayerDied!");
            PlayerDied?.Invoke();
            enabled = false;
            foreach (MeshRenderer renderer in allRenderers)
            {
                renderer.enabled = false;
            }
            deathEffect.Play();
        }

        private void Update()
        {
            transform.position = Vector3.Lerp(transform.position, player.Position.ToUnityVector(), 0.8f);
            transform.rotation = Quaternion.Slerp(transform.rotation, player.Rotation.ToUnityQuaternion(), 0.8f);
        }
    }
}
