using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RoomUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roomNameText;
    [SerializeField] private Button returnToLobbyBtn;

    private void Start()
    {
        GlobalManagers.Instance.GameManager.OnRoomNameReady += SetRoomName;
        
        returnToLobbyBtn.onClick.AddListener(() => GlobalManagers.Instance.NetworkRunnerController.ShutDownRunner());
    }

    private void SetRoomName(string roomName)
    {
        roomNameText.text = roomName;
    }

    private void OnDestroy()
    {
        GlobalManagers.Instance.GameManager.OnRoomNameReady -= SetRoomName;
    }
}
