using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float jumpForce = 1000;

    [Networked(OnChanged = nameof(OnNicknameChanged))] private NetworkString<_8> PlayerName { get; set; }
    [Networked] private NetworkButtons ButtonsPrev { get; set; }
    private Rigidbody2D body;
    private float horizontal;
    private PlayerWeaponController weaponController;
    private PlayerVisualController visualController;
    
    public enum PlayerInputButtons
    {
        None,
        Jump,
        Shoot
    }

    public override void Spawned()
    {
        body = GetComponent<Rigidbody2D>();
        weaponController = GetComponent<PlayerWeaponController>();
        visualController = GetComponent<PlayerVisualController>();
        SetLocalObjects();
    }

    private void SetLocalObjects()
    {
        if(Object.HasInputAuthority)
        {
            playerCamera.SetActive(true);
            var nickname = GlobalManagers.Instance.NetworkRunnerController.LocalPlayerNickname;
            RpcSetNickname(nickname);
        }
        else 
        {
            // if this is not our InputAuthority (aka a proxy)
            // We want to make sure to set the interpolation to snapshots
            // as it will be automatically set to predicted because we are doing full physics prediction
            // that will make sure that lag compensation works properly + be more cost efficient
            GetComponent<NetworkRigidbody2D>().InterpolationDataSource = InterpolationDataSources.Snapshots;
        }
    }

    // Sends RPC to the HOST from a client
    // "sources" define which PEER can send the rpc
    // the RpcTargets defines on wich it is executed
    [Rpc(sources: RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RpcSetNickname(NetworkString<_8> nickname)
    {
        PlayerName = nickname;
    }

    private static void OnNicknameChanged(Changed<PlayerController> changed)
    {
        var nickname = changed.Behaviour.PlayerName;
        changed.Behaviour.SetPlayerNickname(nickname);
    }

    private void SetPlayerNickname(NetworkString<_8> nickname)
    {
        playerNameText.text = nickname + " " + Object.InputAuthority.PlayerId;
    }

    // Happens before anything else Fusion does, every screen refresh;
    // Called at the start of the Fusion Update loop, before the Fusion simulation loop.
    public void BeforeUpdate()
    {
        // We are the local machine
        if (Object.HasInputAuthority)
        {
            const string HORIZONTAL = "Horizontal";
            horizontal = Input.GetAxisRaw(HORIZONTAL);
        }
    }

    // FUN
    public override void FixedUpdateNetwork()
    {
        // will return false if:
        // 1.the client does not have State Authority or InputAuthority
        // 2.the requested type of input does not exist in the simulation
        if(Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input))
        {
            body.velocity = new Vector2(input.HorizontalInput * moveSpeed, body.velocity.y);

            CheckJumpInput(input);
        }

        visualController.UpdateScaleTransforms(body.velocity);
    }

    public override void Render()
    {
        visualController.RenderVisuals(body.velocity, weaponController.IsHoldingShootKey);
    }

    private void CheckJumpInput(PlayerData input)
    {
        var pressed = input.NetworkButtons.GetPressed(ButtonsPrev);
        if(pressed.WasPressed(ButtonsPrev, PlayerInputButtons.Jump))
        {
            body.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
        }

        ButtonsPrev = input.NetworkButtons;
    }

    public PlayerData GetPlayerNetworkInput()
    {
        PlayerData data = new PlayerData();
        data.HorizontalInput = horizontal;
        data.GunPivotRotation = weaponController.LocalQuaternionPivotRotation;
        data.NetworkButtons.Set(PlayerInputButtons.Jump, Input.GetKey(KeyCode.Space));
        data.NetworkButtons.Set(PlayerInputButtons.Shoot, Input.GetButton("Fire1"));
        return data;
    }
}
