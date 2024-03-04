using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private float moveSpeed = 20;
    [SerializeField] private float lifeTimeAmount = 0.8f;

    [Networked] private TickTimer lifeTimeTimer { get; set; }
    [Networked] private NetworkBool didHitSomething { get; set; }

    private Collider2D bulletCollider;

    public override void Spawned()
    {
        bulletCollider = GetComponent<Collider2D>();
        lifeTimeTimer = TickTimer.CreateFromSeconds(Runner, lifeTimeAmount);
    }

    public override void FixedUpdateNetwork()
    {
        CheckIfHitGround();

        if(!lifeTimeTimer.ExpiredOrNotRunning(Runner) && !didHitSomething)
        {
            transform.Translate(transform.right * moveSpeed * Runner.DeltaTime, Space.World);
        }
        
        if(lifeTimeTimer.Expired(Runner) || didHitSomething)
        {
            Runner.Despawn(Object);
        }
    }

    private void CheckIfHitGround()
    {
        var groundCollider = Runner.GetPhysicsScene2D()
            .OverlapBox(transform.position, bulletCollider.bounds.size, 0, groundLayerMask);

        if(groundCollider != default)
        {
            didHitSomething = true;
        }
    }
}
