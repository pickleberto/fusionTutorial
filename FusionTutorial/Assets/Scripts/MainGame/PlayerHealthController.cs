using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

public class PlayerHealthController : NetworkBehaviour
{
    [SerializeField] private Image fillAmountImg;
    [SerializeField] private TextMeshProUGUI healthAmountText;

    [Networked(OnChanged =nameof(HealthAmountChanged))] private int currentHealthAmount { get; set; }

    private const int MAX_HEALTH_AMOUNT = 100;

    public override void Spawned()
    {
        currentHealthAmount = MAX_HEALTH_AMOUNT;
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
            Debug.Log("Local player got hit!");
        }

        if(healthAmount <= 0)
        {
            Debug.Log("player is dead");
        }
    }
}
