using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class PlayerSpawnerController : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef playerNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private Transform[] spawnPoints;
    private Dictionary<PlayerRef, NetworkObject> spawnedPlayers = new Dictionary<PlayerRef, NetworkObject>();

    private void Awake()
    {
        if(GlobalManagers.Instance.PlayerSpawnerController == null)
        {
            GlobalManagers.Instance.PlayerSpawnerController = this;
        }
    }

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

        var index = playerRef.AsIndex % spawnPoints.Length;
        var spawnPosition = spawnPoints[index].transform.position;
        var playerObject = Runner.Spawn(playerNetworkPrefab, spawnPosition, Quaternion.identity, playerRef);

        Runner.SetPlayerObject(playerRef, playerObject);
    }

    public void AddToEntry(PlayerRef playerRef, NetworkObject playerObject)
    {
        spawnedPlayers.TryAdd(playerRef, playerObject);
    }

    public void PlayerLeft(PlayerRef player)
    {
        DespawnPlayer(player);
    }

    private void DespawnPlayer(PlayerRef playerRef)
    {
        if(!Runner.IsServer) return;

        if(spawnedPlayers.TryGetValue(playerRef, out var playerNetworkObject))
        {
            Runner.Despawn(playerNetworkObject);
        }

        Runner.SetPlayerObject(playerRef, null);
    }

    public Vector2 GetRandomSpawnPos()
    {
        int index = Random.Range(0, spawnPoints.Length - 1);
        return spawnPoints[index].position;
    }
}
