using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{
    public bool AcceptAnyInput => PlayerIsAlive && !GameManager.MatchIsOver;

    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float jumpForce = 1000;

    [Header("Grounded Vars")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundDetectionObj;

    [Networked] public NetworkBool PlayerIsAlive { get; private set; }
    [Networked] public TickTimer RespawnTimer { get; private set; }
    [Networked(OnChanged = nameof(OnNicknameChanged))] private NetworkString<_8> playerName { get; set; }
    [Networked] private NetworkButtons buttonsPrev { get; set; }
    [Networked] private Vector2 nextSpawnPos { get; set; }
    [Networked] private NetworkBool isGrounded { get; set; }
    [Networked] private TickTimer repositionTimer { get; set; }

    private Rigidbody2D body;
    private float horizontal;
    private PlayerWeaponController weaponController;
    private PlayerVisualController visualController;
    private PlayerHealthController healthController;
    private NetworkRigidbody2D netBody;
    
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
        healthController = GetComponent<PlayerHealthController>();
        netBody = GetComponent<NetworkRigidbody2D>();
        SetLocalObjects();
        PlayerIsAlive = true;
    }

    private void SetLocalObjects()
    {
        if(Object.HasInputAuthority)
        {
            playerCamera.transform.SetParent(null);
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
        playerName = nickname;
    }

    private static void OnNicknameChanged(Changed<PlayerController> changed)
    {
        var nickname = changed.Behaviour.playerName;
        changed.Behaviour.SetPlayerNickname(nickname);
    }

    private void SetPlayerNickname(NetworkString<_8> nickname)
    {
        playerNameText.text = nickname + " " + Object.InputAuthority.PlayerId;
    }

    public void KillPlayer()
    {
        const int RESPAWN_TIME_SECONDS = 5;
        if (Runner.IsServer)
        {
            nextSpawnPos = GlobalManagers.Instance.PlayerSpawnerController.GetRandomSpawnPos();
            repositionTimer = TickTimer.CreateFromSeconds(Runner, RESPAWN_TIME_SECONDS - 1);
        }

        PlayerIsAlive = false;
        body.simulated = false;
        visualController.TriggerDieAnimation();

        RespawnTimer = TickTimer.CreateFromSeconds(Runner, RESPAWN_TIME_SECONDS);
    }

    // Happens before anything else Fusion does, every screen refresh;
    // Called at the start of the Fusion Update loop, before the Fusion simulation loop.
    public void BeforeUpdate()
    {
        // We are the local machine
        if (Object.HasInputAuthority && AcceptAnyInput)
        {
            const string HORIZONTAL = "Horizontal";
            horizontal = Input.GetAxisRaw(HORIZONTAL);
        }
    }

    // FUN
    public override void FixedUpdateNetwork()
    {
        CheckRespawnTimer();

        // will return false if:
        // 1.the client does not have State Authority or InputAuthority
        // 2.the requested type of input does not exist in the simulation
        if(Runner.TryGetInputForPlayer<PlayerData>(Object.InputAuthority, out var input) && AcceptAnyInput)
        {
            body.velocity = new Vector2(input.HorizontalInput * moveSpeed, body.velocity.y);

            CheckJumpInput(input);

            buttonsPrev = input.NetworkButtons;
        }

        visualController.UpdateScaleTransforms(body.velocity);
    }

    private void CheckRespawnTimer()
    {
        if (PlayerIsAlive) return;

        // Will only run on the server
        if(repositionTimer.Expired(Runner))
        {
            repositionTimer = TickTimer.None;
            netBody.TeleportToPosition(nextSpawnPos);
        }

        if(RespawnTimer.Expired(Runner))
        {
            RespawnTimer = TickTimer.None;
            RespawnPlayer();
        }
    }

    public void RespawnPlayer()
    {
        PlayerIsAlive = true;
        body.simulated = true;
        //body.position = nextSpawnPos;
        visualController.TriggerRespawnAnimation();
        healthController.ResetHealthToMax();
    }

    public override void Render()
    {
        visualController.RenderVisuals(body.velocity, weaponController.IsHoldingShootKey);
    }

    private void CheckJumpInput(PlayerData input)
    {
        isGrounded = (bool)Runner.GetPhysicsScene2D().OverlapBox(
            groundDetectionObj.position, groundDetectionObj.localScale, 0, groundLayer);

        if(isGrounded)
        {
            var pressed = input.NetworkButtons.GetPressed(buttonsPrev);
            if(pressed.WasPressed(buttonsPrev, PlayerInputButtons.Jump))
            {
                body.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
            }
        }
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

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        // Avoid the pooling of player objects
        GlobalManagers.Instance.ObjectPoolingManager.RemoveNetworkObjectFromDict(Object);
        Destroy(gameObject);
    }
}
