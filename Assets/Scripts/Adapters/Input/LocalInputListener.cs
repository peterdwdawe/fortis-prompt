using Shared.Input;
using System;
using UnityEngine;

namespace Adapters.Input
{
    public class LocalInputListener : MonoBehaviour, IInputListener
    {
        //Currently never called, but in the future can be used by server to correct invalid movement submitted by player
        public event Action<System.Numerics.Vector3, System.Numerics.Quaternion> OnTransformUpdated;

        public event Action OnShootLocal;

        public System.Numerics.Vector2 Movement { get; private set; }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyUp(KeyCode.Space))
            {
                OnShootLocal?.Invoke();
            }

            Movement = new System.Numerics.Vector2(UnityEngine.Input.GetAxis("Horizontal"), UnityEngine.Input.GetAxis("Vertical"));
        }
    }
}
