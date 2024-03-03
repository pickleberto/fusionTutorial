using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerWeaponController : NetworkBehaviour, IBeforeUpdate
{
    public Quaternion LocalQuaternionPivotRotation { get; private set; }
    [SerializeField] private Camera localCamera;
    [SerializeField] private Transform pivotToRotate;
    
    // synchronized value
    [Networked] private Quaternion currentRotation { get; set; }

    public void BeforeUpdate()
    {
        if(Runner.LocalPlayer == Object.HasInputAuthority)
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
            currentRotation = input.GunPivotRotation;
        }

        // this runs in the proxies as well, with the 
        // synchronized value [Networked]
        pivotToRotate.rotation = currentRotation;
    }
}
