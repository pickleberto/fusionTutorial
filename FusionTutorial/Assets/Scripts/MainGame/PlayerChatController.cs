using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.EventSystems;

public class PlayerChatController : NetworkBehaviour
{
    [Networked] public bool IsTyping { get; private set; }
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
            inputField.onSelect.AddListener((_) => Rpc_UpdateServerTypingStatus(true));
            inputField.onDeselect.AddListener((_) => Rpc_UpdateServerTypingStatus(false));

            inputField.onSubmit.AddListener(OnInputFieldSubmit);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void Rpc_UpdateServerTypingStatus(bool isTyping)
    {
        IsTyping = isTyping;
    }

    private void OnInputFieldSubmit(string inText)
    {
        if (string.IsNullOrEmpty(inText)) return;

        RpcSetBubbleSpeech(inText);

        inputField.text = string.Empty;
        var eventSystem = EventSystem.current;
        if (!eventSystem.alreadySelecting) eventSystem.SetSelectedGameObject(null);
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.All)]
    private void RpcSetBubbleSpeech(NetworkString<_64> msg)
    {
        bubbleText.text = msg.Value;

        bubbleAnimator.SetTrigger(openBubbleTriggerHash);
    }
}
