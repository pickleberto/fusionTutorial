using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class RespawnPanel : NetworkBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private TextMeshProUGUI respawnAmountText;
    [SerializeField] private GameObject childObj;

    public override void Spawned()
    {
        Runner.SetIsSimulated(Object, true);
    }

    public override void FixedUpdateNetwork()
    {
        if (!playerController.Object.HasInputAuthority) return;

        var timerIsRunning = playerController.RespawnTimer.IsRunning;
        childObj.SetActive(timerIsRunning);

        if(timerIsRunning && playerController.RespawnTimer.RemainingTime(Runner).HasValue)
        {
            var time = playerController.RespawnTimer.RemainingTime(Runner).Value;
            var roundToInt = Mathf.RoundToInt(time);
            respawnAmountText.text = roundToInt.ToString();
        }

    }
}
