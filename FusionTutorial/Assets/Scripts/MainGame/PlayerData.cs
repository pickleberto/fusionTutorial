using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct PlayerData : INetworkInput
{
    public float HorizontalInput;
    public Quaternion GunPivotRotation;
    public NetworkButtons NetworkButtons;
}
