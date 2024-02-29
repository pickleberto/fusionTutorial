using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerSpawnerController : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef playerNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private Transform[] spawnPoints;

    public override void Spawned()
    {
        if(!Runner.IsServer) return;

        foreach (var player in Runner.ActivePlayers)
        {
            SpawnPlayer(player);
        }
    }

    public void PlayerJoined(PlayerRef player)
    {
        SpawnPlayer(player);
    }

    private void SpawnPlayer(PlayerRef playerRef)
    {
        if(!Runner.IsServer) return;
        
        var index = playerRef % spawnPoints.Length;
        var spawnPosition = spawnPoints[index].transform.position;
        var playerObject = Runner.Spawn(playerNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);

        Runner.SetPlayerObject(playerRef, playerObject);
    }

    public void PlayerLeft(PlayerRef player)
    {
        DespawnPlayer(player);
    }

    private void DespawnPlayer(PlayerRef playerRef)
    {
        if(!Runner.IsServer) return;

        if(Runner.TryGetPlayerObject(playerRef, out var playerNetworkObject))
        {
            Runner.Despawn(playerNetworkObject);
        }

        Runner.SetPlayerObject(playerRef, null);
    }
}
