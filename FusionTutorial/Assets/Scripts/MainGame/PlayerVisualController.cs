using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVisualController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private Transform gunPivot;
    [SerializeField] private Transform worldCanvas;
    
    private readonly int isMovingHash = Animator.StringToHash("IsWalking");
    private readonly int isShootingHash = Animator.StringToHash("IsShooting");
    private readonly int dieTriggerHash = Animator.StringToHash("Die");

    private bool isFacingRight = true;
    private Vector3 originalPlayerScale;
    private Vector3 originalGunPivotScale;
    private Vector3 originalCanvasScale;

    private bool init = false;

    private void Start() 
    {
        originalPlayerScale = transform.localScale;
        originalGunPivotScale = gunPivot.localScale;
        originalCanvasScale = worldCanvas.localScale;

        const int SHOOTING_LAYER_INDEX = 1;
        animator.SetLayerWeight(SHOOTING_LAYER_INDEX, 1);

        init = true;
    }
    
    public void TriggerDieAnimation()
    {
        animator.SetTrigger(dieTriggerHash);
    }

    public void RenderVisuals(Vector2 velocity, bool isShooting)
    {
        if(!init) return;

        bool isMoving = velocity.x > 0.1f || velocity.x < -0.1f;
        animator.SetBool(isMovingHash, isMoving);
        animator.SetBool(isShootingHash, isShooting);
    }

    public void UpdateScaleTransforms(Vector2 velocity)
    {
        if(!init) return;

        if(velocity.x > 0.1f)
        {
            isFacingRight = true;
        }
        else if(velocity.x < -0.1f)
        {
            isFacingRight = false;
        }

        SetObjectTransformScale(gameObject, originalPlayerScale);
        SetObjectTransformScale(gunPivot.gameObject, originalGunPivotScale);
        SetObjectTransformScale(worldCanvas.gameObject, originalCanvasScale);
    }

    private void SetObjectTransformScale(GameObject obj, Vector3 originalScale)
    {
        var xValue = isFacingRight? originalScale.x : -originalScale.x;
        obj.transform.localScale = new Vector3(xValue, originalScale.y, originalScale.z);
    }
}
