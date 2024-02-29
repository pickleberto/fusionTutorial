using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class MiddleSectionPanel : LobbyPanelBase
{
    [Header("Middle Section Panel vars")]
    [SerializeField] private Button joinRandomRoomBtn;
    [SerializeField] private Button joinRoomByArgBtn;
    [SerializeField] private Button createRoomBtn;
    [SerializeField] private TMP_InputField joinRoomByArgInputField;
    [SerializeField] private TMP_InputField createRoomInputField;

    private NetworkRunnerController networkRunnerController;

    public override void InitPanel(LobbyUIManager lobbyUIManager)
    {
        base.InitPanel(lobbyUIManager);

        networkRunnerController = GlobalManagers.Instance.NetworkRunnerController;
        joinRandomRoomBtn.onClick.AddListener(JoinRandomRoom);
        joinRoomByArgBtn.onClick.AddListener(() => CreateRoom(GameMode.Client, joinRoomByArgInputField.text));
        createRoomBtn.onClick.AddListener(() => CreateRoom(GameMode.Host, createRoomInputField.text));
    }

    private void CreateRoom(GameMode mode, string roomName)
    {
        if(roomName.Length >= 2)
        {
            Debug.Log($"----------------{mode}--------------------");
            networkRunnerController.StartGame(mode, roomName);
        }
    }

    private void JoinRandomRoom()
    {
        Debug.Log($"----------------JoinRandomRoom!--------------------");
        networkRunnerController.StartGame(GameMode.AutoHostOrClient, string.Empty);
    }
}
