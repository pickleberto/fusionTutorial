using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class PlayerChatController : NetworkBehaviour
{
    public static bool IsTyping { get; private set; }
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Animator bubbleAnimator;
    [SerializeField] private TextMeshProUGUI bubbleText;

    private readonly int openBubbleTriggerHash = Animator.StringToHash("Open");

    public override void Spawned()
    {
        var isLocalPlayer = Object.InputAuthority == Runner.LocalPlayer;
        gameObject.SetActive(isLocalPlayer);

        if(isLocalPlayer)
        {
            inputField.onSelect.AddListener((_) => IsTyping = true);
            inputField.onDeselect.AddListener((_) => IsTyping = false);

            inputField.onSubmit.AddListener(OnInputFieldSubmit);
        }
    }

    private void OnInputFieldSubmit(string inText)
    {
        if (string.IsNullOrEmpty(inText)) return;

        RpcSetBubbleSpeech(inText);
        //inputField.text = "";
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RpcSetBubbleSpeech(NetworkString<_64> msg)
    {
        bubbleText.text = msg.Value;

        bubbleAnimator.SetTrigger(openBubbleTriggerHash);
    }
}
