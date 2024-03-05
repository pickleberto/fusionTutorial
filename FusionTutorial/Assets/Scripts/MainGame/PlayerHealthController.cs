using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

public class PlayerHealthController : NetworkBehaviour
{
    [SerializeField] private Animator bloodScreenHitAnimator;
    [SerializeField] private PlayerCameraController cameraController;
    [SerializeField] private Image fillAmountImg;
    [SerializeField] private TextMeshProUGUI healthAmountText;

    [Networked(OnChanged =nameof(HealthAmountChanged))] private int currentHealthAmount { get; set; }

    private const int MAX_HEALTH_AMOUNT = 100;
    private PlayerController playerController;

    public override void Spawned()
    {
        currentHealthAmount = MAX_HEALTH_AMOUNT;
        playerController = GetComponent<PlayerController>();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_ReducePlayerHealth(int damage)
    {
        currentHealthAmount -= damage;
    }

    private static void HealthAmountChanged(Changed<PlayerHealthController> changed)
    {
        var currrentHealth = changed.Behaviour.currentHealthAmount;
        changed.LoadOld();
        var oldHealth = changed.Behaviour.currentHealthAmount;

        if(currrentHealth != oldHealth)
        {
            changed.Behaviour.UpdateVisuals(currrentHealth);
            
            // We did not respawn or just spawned
            if(currrentHealth != MAX_HEALTH_AMOUNT)
            {
                changed.Behaviour.PlayerGotHit(currrentHealth);
            }
        }
    }

    private void UpdateVisuals(int healthAmount)
    {
        var health = (float)healthAmount / MAX_HEALTH_AMOUNT;
        fillAmountImg.fillAmount = health;
        healthAmountText.text = $"{healthAmount}/{MAX_HEALTH_AMOUNT}";
    }

    private void PlayerGotHit(int healthAmount)
    {
        if(Object.HasInputAuthority)
        {
            const string BLOOD_HIT_CLIP_NAME = "BloodScreenHit";
            bloodScreenHitAnimator.Play(BLOOD_HIT_CLIP_NAME);

            cameraController.ShakeCamera();
        }

        if(healthAmount <= 0)
        {
            playerController.KillPlayer();
        }
    }
}
