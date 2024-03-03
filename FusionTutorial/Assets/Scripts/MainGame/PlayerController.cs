using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerController : NetworkBehaviour, IBeforeUpdate
{

    [SerializeField] private float moveSpeed = 6;
    [SerializeField] private float jumpForce = 1000;
    [Networked] private NetworkButtons ButtonsPrev { get; set; }
    private Rigidbody2D rigidbody;
    private float horizontal;
    
    private enum PlayerInputButtons
    {
        None,
        Jump
    }

    public override void Spawned()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    // Happens before anything else Fusion does, every screen refresh;
    // Called at the start of the Fusion Update loop, before the Fusion simulation loop.
    public void BeforeUpdate()
    {
        // We are the local machine
        if(Runner.LocalPlayer == Object.HasInputAuthority)
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
            rigidbody.velocity = new Vector2(input.HorizontalInput * moveSpeed, rigidbody.velocity.y);

            CheckJumpInput(input);
        }
    }

    private void CheckJumpInput(PlayerData input)
    {
        var pressed = input.NetworkButtons.GetPressed(ButtonsPrev);
        if(pressed.WasPressed(ButtonsPrev, PlayerInputButtons.Jump))
        {
            rigidbody.AddForce(Vector2.up * jumpForce, ForceMode2D.Force);
        }

        ButtonsPrev = input.NetworkButtons;
    }

    public PlayerData GetPlayerNetworkInput()
    {
        PlayerData data = new PlayerData();
        data.HorizontalInput = horizontal;
        data.NetworkButtons.Set(PlayerInputButtons.Jump, Input.GetKey(KeyCode.Space));
        return data;
    }
}
