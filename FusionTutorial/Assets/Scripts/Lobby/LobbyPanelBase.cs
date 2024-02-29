using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPanelBase : MonoBehaviour
{
    public enum LobbyPanelType
    {
        None,
        CreateNicknamePanel,
        MiddleSectionPanel
    }

    [field:SerializeField, Header("Lobby Panel Vars")]
    public LobbyPanelType PanelType{ get; private set; }
    [SerializeField] private Animator panelAnimator;
    protected LobbyUIManager lobbyUIManager;

    public virtual void InitPanel(LobbyUIManager uiManager)
    {
        lobbyUIManager = uiManager;
    }

    public void ShowPanel()
    {
        this.gameObject.SetActive(true);
        const string POP_IN_CLIP_NAME = "In";
        panelAnimator.Play(POP_IN_CLIP_NAME);
    }
   
   protected void ClosePanel()
   {
        const string POP_OUT_CLIP_NAME = "Out";
        StartCoroutine(Utils.PlayAnimAndSetStateWhenFinished(
            gameObject, panelAnimator, POP_OUT_CLIP_NAME, false));
   }
}
