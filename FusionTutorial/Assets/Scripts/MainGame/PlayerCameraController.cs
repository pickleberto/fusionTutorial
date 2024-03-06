using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineImpulseSource impulseSource;
    [SerializeField] private CinemachineConfiner2D confiner2D;

    private void Start()
    {
        confiner2D.m_BoundingShape2D = GlobalManagers.Instance.GameManager.CameraBoundaries;
    }

    public void ShakeCamera()
    {
        impulseSource.GenerateImpulse();
    }
}
