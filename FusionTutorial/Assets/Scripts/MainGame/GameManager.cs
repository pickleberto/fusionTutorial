using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class GameManager : NetworkBehaviour
{
    public event Action OnGameIsOver;
    public event Action<string> OnRoomNameReady;
    public static bool MatchIsOver { get; private set; }
    [field: SerializeField] public Collider2D CameraBoundaries { get; private set; }
    [SerializeField] private Camera cam;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float matchTimerAmount = 60;

    [Networked] private TickTimer matchTimer { get; set; }

    private void Awake()
    {
        if(GlobalManagers.Instance != null)
        {
            GlobalManagers.Instance.GameManager = this;
        }
    }

    public override void Spawned()
    {
        Runner.SetIsSimulated(Object, true);

        MatchIsOver = false;
        cam.gameObject.SetActive(false);
        matchTimer = TickTimer.CreateFromSeconds(Runner, matchTimerAmount);
        OnRoomNameReady?.Invoke(Runner.SessionInfo.Name);
    }

    public override void FixedUpdateNetwork()
    {
        if(!matchTimer.Expired(Runner) && matchTimer.RemainingTime(Runner).HasValue)
        {
            var timeSpan = TimeSpan.FromSeconds(matchTimer.RemainingTime(Runner).Value);
            timerText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
        }
        else if (matchTimer.Expired(Runner))
        {
            MatchIsOver = true;
            matchTimer = TickTimer.None;
            Debug.Log("Match timer has ended");
            OnGameIsOver?.Invoke();
        }
    }
}
