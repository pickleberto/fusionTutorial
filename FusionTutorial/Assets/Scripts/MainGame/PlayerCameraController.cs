using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;

    public void ShakeCamera()
    {
        impulseSource.GenerateImpulse();
    }
}
