using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class Bullet : NetworkBehaviour
{
    [SerializeField] private LayerMask playerLayerMask;
    [SerializeField] private LayerMask groundLayerMask;
    [SerializeField] private int bulletDamage = 10;
    [SerializeField] private float moveSpeed = 20;
    [SerializeField] private float lifeTimeAmount = 0.8f;

    [Networked] private TickTimer lifeTimeTimer { get; set; }
    [Networked] private NetworkBool didHitSomething { get; set; }

    private Collider2D bulletCollider;
    private List<LagCompensatedHit> hits = new List<LagCompensatedHit>();

    public override void Spawned()
    {
        bulletCollider = GetComponent<Collider2D>();
        lifeTimeTimer = TickTimer.CreateFromSeconds(Runner, lifeTimeAmount);
    }

    public override void FixedUpdateNetwork()
    {
        if(!didHitSomething)
        {
            CheckIfHitGround();
            CheckIfWeHitAPlayer();
        }

        if(!lifeTimeTimer.ExpiredOrNotRunning(Runner) && !didHitSomething)
        {
            transform.Translate(transform.right * moveSpeed * Runner.DeltaTime, Space.World);
        }
        
        if(lifeTimeTimer.Expired(Runner) || didHitSomething)
        {
            lifeTimeTimer = TickTimer.None;
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
    
    private void CheckIfWeHitAPlayer()
    {
        Runner.LagCompensation.OverlapBox(transform.position, bulletCollider.bounds.size,
            Quaternion.identity, Object.InputAuthority, hits, playerLayerMask);

        if(hits.Count > 0)
        {
            foreach(var item in hits)
            {
                if(item.Hitbox != null)
                {
                    var player = item.Hitbox.GetComponentInParent<NetworkObject>();
                    var didNotHitOurOwnPlayer = player.InputAuthority.PlayerId != Object.InputAuthority.PlayerId;
                    if(didNotHitOurOwnPlayer)
                    {
                        if(Runner.IsServer)
                        {
                            player.GetComponent<PlayerHealthController>().Rpc_ReducePlayerHealth(bulletDamage);
                        }
                        didHitSomething = true;
                        break;
                    }
                }
            }
        }
    }
}
