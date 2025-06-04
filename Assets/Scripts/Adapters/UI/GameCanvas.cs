using Adapters.Character;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
