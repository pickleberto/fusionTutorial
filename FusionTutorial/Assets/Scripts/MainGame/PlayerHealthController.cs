using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Fusion;
using TMPro;

public class PlayerHealthController : NetworkBehaviour
{
    [SerializeField] private LayerMask deathGroundLayerMask;
    [SerializeField] private Animator bloodScreenHitAnimator;
    [SerializeField] private PlayerCameraController cameraController;
    [SerializeField] private Image fillAmountImg;
    [SerializeField] private TextMeshProUGUI healthAmountText;

    [Networked] private int currentHealthAmount { get; set; }

    private const int MAX_HEALTH_AMOUNT = 100;
    private PlayerController playerController;
    private Collider2D playerCollider;
    private ChangeDetector changeDetector;

    public override void Spawned()
    {
        changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        currentHealthAmount = MAX_HEALTH_AMOUNT;
        playerController = GetComponent<PlayerController>();
        playerCollider = GetComponent<Collider2D>();
    }

    public override void FixedUpdateNetwork()
    {
        if (Runner.IsServer && playerController.PlayerIsAlive)
        {
            var didHitCollider = Runner.GetPhysicsScene2D().OverlapBox(
                transform.position, playerCollider.bounds.size, 0, deathGroundLayerMask);

            if (didHitCollider != default)
                Rpc_ReducePlayerHealth(MAX_HEALTH_AMOUNT);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.StateAuthority)]
    public void Rpc_ReducePlayerHealth(int damage)
    {
        currentHealthAmount -= damage;
    }

    public override void Render()
    {
        foreach(var change in changeDetector.DetectChanges(this, out var prev, out var current))
        {
            switch (change)
            {
                case nameof(currentHealthAmount):
                    var reader = GetPropertyReader<int>(nameof(currentHealthAmount));
                    var (oldHealth, currentHealth) = reader.Read(prev, current);
                    HealthAmountChanged(oldHealth, currentHealth);
                    break;
            }
        }
    }

    private void HealthAmountChanged(int oldHealth, int currrentHealth)
    {
        if(currrentHealth != oldHealth)
        {
            UpdateVisuals(currrentHealth);
            
            // We did not respawn or just spawned
            if(currrentHealth != MAX_HEALTH_AMOUNT)
            {
                PlayerGotHit(currrentHealth);
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

    public void ResetHealthToMax()
    {
        currentHealthAmount = MAX_HEALTH_AMOUNT;
    }
}
