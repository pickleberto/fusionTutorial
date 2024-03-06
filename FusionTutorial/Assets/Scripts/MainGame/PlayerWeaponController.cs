using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerWeaponController : NetworkBehaviour, IBeforeUpdate
{
    [SerializeField] private NetworkPrefabRef bulletPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private Transform firePointPos;
    [SerializeField] private float delayBetweenShots = 0.18f;
    [SerializeField] private ParticleSystem muzzleEffect;
    [SerializeField] private Camera localCamera;
    [SerializeField] private Transform pivotToRotate;
    public Quaternion LocalQuaternionPivotRotation { get; private set; }
    
    // synchronized values
    [Networked] private Quaternion currentRotation { get; set; }
    [Networked] private NetworkButtons buttonsPrev { get; set; }
    [Networked] private TickTimer shootCooldown { get; set; }
    [Networked(OnChanged =nameof(OnMuzzleEffectStateChanged))] private NetworkBool playMuzzleEffect { get; set; }
    [Networked, HideInInspector] public NetworkBool IsHoldingShootKey { get; private set; }

    private PlayerController playerController;

    public override void Spawned()
    {
        playerController = GetComponent<PlayerController>();
    }

    public void BeforeUpdate()
    {
        if(Object.HasInputAuthority && playerController.AcceptAnyInput)
        {
            var direction = localCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position;
            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            LocalQuaternionPivotRotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    // FUN
    public override void FixedUpdateNetwork()
    {
        if(Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input))
        {
            // this only runs in the local client (has the input authority)
            if(playerController.AcceptAnyInput)
            {
                CheckShootInput(input);
                currentRotation = input.GunPivotRotation;
                buttonsPrev = input.NetworkButtons;
            }
            else
            {
                IsHoldingShootKey = false;
                playMuzzleEffect = false;
                buttonsPrev = default;
                currentRotation = Quaternion.identity;
            }
        }

        // this runs in the proxies as well, with the 
        // synchronized value [Networked]
        pivotToRotate.rotation = currentRotation;
    }

    private void CheckShootInput(PlayerData input)
    {
        var currentBtns = input.NetworkButtons.GetPressed(buttonsPrev);
        IsHoldingShootKey = currentBtns.WasReleased(buttonsPrev, PlayerController.PlayerInputButtons.Shoot);

        if (currentBtns.WasReleased(buttonsPrev, PlayerController.PlayerInputButtons.Shoot) 
            && shootCooldown.ExpiredOrNotRunning(Runner))
        {
            playMuzzleEffect = true;
            shootCooldown = TickTimer.CreateFromSeconds(Runner, delayBetweenShots);

            Runner.Spawn(bulletPrefab, firePointPos.position, firePointPos.rotation, Object.InputAuthority);
        }
        else
        {
            playMuzzleEffect = false;
        }
    }

    private static void OnMuzzleEffectStateChanged(Changed<PlayerWeaponController> changed)
    {
        var currentState = changed.Behaviour.playMuzzleEffect;
        
        changed.LoadOld();
        var oldState = changed.Behaviour.playMuzzleEffect;

        if(oldState != currentState)
        {
            changed.Behaviour.PlayOrStopMuzzleEffect(currentState);
        }
    }

    private void PlayOrStopMuzzleEffect(bool play)
    {
        if(play)
        {
            muzzleEffect.Play();
        }
        else
        {
            muzzleEffect.Stop();
        }
    }
}
