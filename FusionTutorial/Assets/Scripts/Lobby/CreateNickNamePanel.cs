using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateNickNamePanel : LobbyPanelBase
{
    [Header("Create Nickname Panel Vars")]
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button createNicknameBtn;

    private const int MIN_CHARS_FOR_NICKNAME = 2;

    public override void InitPanel(LobbyUIManager lobbyUIManager)
    {
        base.InitPanel(lobbyUIManager);
        
        createNicknameBtn.interactable = false;
        createNicknameBtn.onClick.AddListener(OnClickCreateNickname);

        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    private void OnInputValueChanged(string inputStr)
    {
        createNicknameBtn.interactable = inputStr.Length >= MIN_CHARS_FOR_NICKNAME;
    }

    private void OnClickCreateNickname()
    {
        var nickname = inputField.text;
        if(nickname.Length >= MIN_CHARS_FOR_NICKNAME)
        {
            base.ClosePanel();
            lobbyUIManager.ShowPanel(LobbyPanelType.MiddleSectionPanel);
        }
    }
}
