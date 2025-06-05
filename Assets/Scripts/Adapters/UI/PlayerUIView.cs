using Adapters.Character;
using Shared.Player;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIView : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI playerID;
    [SerializeField] Slider hpSlider;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Vector3 positionOffset = Vector3.up;
    PlayerView player;
    GameObject root;
    Camera cam;
    RectTransform rectTransform;
    RectTransform parentRectTransform;

    internal void Setup(GameObject root, PlayerView player)
    {
        this.root = root;
        this.player = player;
        cam = Camera.main;

        player.PlayerSpawned += PlayerSpawned;
        player.PlayerDied += PlayerDied;
        player.PlayerDestroyed += PlayerDestroyed;
        player.player.HPSet += SetHP;

        hpSlider.maxValue = player.player.MaxHP;
        hpSlider.minValue = 0;
        hpSlider.value = player.player.HP;
        canvasGroup.alpha = 0f;
        playerID.text = $"Player {player.player.ID}";
    }

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        parentRectTransform = rectTransform.parent as RectTransform;
    }

    private void SetHP(IPlayer player)
    {
        hpSlider.value = player.HP;
    }

    private void PlayerDestroyed()
    {
        player.PlayerSpawned -= PlayerSpawned;
        player.PlayerDied -= PlayerDied;
        player.PlayerDestroyed -= PlayerDestroyed;
        player.player.HPSet -= SetHP;
        Destroy(root);
    }

    private void PlayerDied()
    {
        canvasGroup.alpha = 0f;
    }

    private void PlayerSpawned()
    {
        canvasGroup.alpha = 1f;
    }

    private void LateUpdate()
    {
        Vector3 screenPoint = cam.WorldToScreenPoint(player.transform.position + positionOffset);
        screenPoint.z = 0;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, screenPoint, null, out Vector2 scaledScreenPos);

        rectTransform.anchoredPosition = scaledScreenPos;
    }
}
