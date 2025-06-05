using Adapters.Character;
using UnityEngine;

public class GameCanvas : MonoBehaviour
{
    private void Awake()
    {
        PlayerView.PlayerInstantiated += PlayerInstantiated;
    }

    private void PlayerInstantiated(PlayerView player)
    {
        var hpRoot = Instantiate(Resources.Load<GameObject>("PlayerHP"), transform);
        var hpBar = hpRoot.GetComponentInChildren<PlayerUIView>();
        hpBar.Setup(hpRoot, player);
    }
}
