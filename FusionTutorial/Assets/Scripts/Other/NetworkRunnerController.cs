using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;

public class NetworkRunnerController : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner networkRunnerPrefab;
    public event Action OnStartedRunnerConnection;
    public event Action OnPlayerJoinedSuccessfully;

    private NetworkRunner networkRunnerInstance;

    public async void StartGame(GameMode mode, string roomName)
    {
        OnStartedRunnerConnection?.Invoke();

        if(networkRunnerInstance == null)
        {
            networkRunnerInstance = Instantiate(networkRunnerPrefab);
        }

        //Register so we will get the callbacks as well
        networkRunnerInstance.AddCallbacks(this);

        //networkRunnerInstance.ProvideInput = true;

        var startGameArgs = new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomName,
            PlayerCount = 4,
            SceneManager = networkRunnerInstance.GetComponent<INetworkSceneManager>()
        };

        var result = await networkRunnerInstance.StartGame(startGameArgs);

        if(result.Ok)
        {
            const string SCENE_NAME = "MainGame";
            networkRunnerInstance.SetActiveScene(SCENE_NAME);
        }
        else
        {
            Debug.Log($"Failed to start: {result.ShutdownReason}");
        }
    }

    public void ShutDownRunner()
    {
        networkRunnerInstance.Shutdown();
    }

 	//Callback when NetworkRunner successfully connects to a server or host.
    public void OnConnectedToServer (NetworkRunner runner)
    {Debug.Log("OnConnectedToServer");}
 
 	//Callback when NetworkRunner fails to connect to a server or host.
    public void OnConnectFailed (NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {Debug.Log("OnConnectFailed");}
 
 	//Callback when NetworkRunner receives a Connection Request from a Remote Client.
    public void OnConnectRequest (NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {Debug.Log("OnConnectRequest");}
 
 	//Callback is invoked when the Authentication procedure returns a response from the Authentication Server.
    public void OnCustomAuthenticationResponse (NetworkRunner runner, Dictionary< string, object > data)
    {Debug.Log("OnCustomAuthenticationResponse");}
 
 	//Callback when NetworkRunner disconnects from a server or host.
	public void OnDisconnectedFromServer (NetworkRunner runner)
    {Debug.Log("OnDisconnectedFromServer");}
 	
    //Callback is invoked when the Host Migration process has started.
	public void OnHostMigration (NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {Debug.Log("OnHostMigration");}
	
 	//Callback from NetworkRunner that polls for user inputs. The NetworkInput that is supplied expects:
    public void OnInput (NetworkRunner runner, NetworkInput input)
    {Debug.Log("OnInput");}
	
    public void OnInputMissing (NetworkRunner runner, PlayerRef player, NetworkInput input)
    {Debug.Log("OnInputMissing");}
 	
    // Callback from a NetworkRunner when a new player has joined.
	public void OnPlayerJoined (NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("OnPlayerJoined");
        OnPlayerJoinedSuccessfully?.Invoke();
    }
 	
    //Callback from a NetworkRunner when a player has disconnected.
	public void OnPlayerLeft (NetworkRunner runner, PlayerRef player)
    {Debug.Log("OnPlayerLeft");}
	
    public void OnReliableDataReceived (NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {Debug.Log("OnReliableDataReceived");}

	public void OnSceneLoadDone (NetworkRunner runner)
    {Debug.Log("OnSceneLoadDone");}

	public void OnSceneLoadStart (NetworkRunner runner)
    {Debug.Log("OnSceneLoadStart");}

 	//This callback is invoked when a new List of Sessions is received from Photon Cloud.
	public void OnSessionListUpdated (NetworkRunner runner, List<SessionInfo> sessionList)
    {Debug.Log("OnSessionListUpdated");}

 	//Called when the runner is shutdown.
	public void OnShutdown (NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("OnShutdown");

        const string LOBBY_SCENE = "Lobby";
        SceneManager.LoadScene(LOBBY_SCENE);
    }

	public void OnUserSimulationMessage (NetworkRunner runner, SimulationMessagePtr message)
    {Debug.Log("OnUserSimulationMessage");}
}
