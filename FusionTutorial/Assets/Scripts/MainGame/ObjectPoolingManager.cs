using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ObjectPoolingManager : MonoBehaviour, INetworkObjectProvider
{
    // key: prefab; value: list of its instances
    private Dictionary<INetworkPrefabSource, List<NetworkObject>> instantiatedPrefabs = new();

    private void Start()
    {
        if(GlobalManagers.Instance != null)
        {
            GlobalManagers.Instance.ObjectPoolingManager = this;
        }
    }

    // Called once runner.Spawn() is called
    NetworkObjectAcquireResult INetworkObjectProvider.AcquirePrefabInstance(NetworkRunner runner, in NetworkPrefabAcquireContext context, out NetworkObject result)
    {
        NetworkObject networkObject = null;
        NetworkPrefabId prefabId = context.PrefabId;
        INetworkPrefabSource prefabSource = NetworkProjectConfig.Global.PrefabTable.GetSource(prefabId);

        instantiatedPrefabs.TryGetValue(prefabSource, out var networkObjectsList);

        bool foundMatch = false;
        if(networkObjectsList?.Count > 0)
        {
            foreach(var item in networkObjectsList)
            {
                if(item != null && !item.gameObject.activeSelf)
                {
                    networkObject = item;
                    foundMatch = true;
                    break;
                }
            }
        }

        if(!foundMatch)
        {
            networkObject = CreateObjectInstance(prefabSource);
        }

        result = networkObject;
        return NetworkObjectAcquireResult.Success;
    }

    private NetworkObject CreateObjectInstance(INetworkPrefabSource prefabSource)
    {
        var obj = Instantiate(prefabSource.WaitForResult(), Vector3.zero, Quaternion.identity);

        if(instantiatedPrefabs.TryGetValue(prefabSource, out var instances))
        {
            instances.Add(obj);
        }
        else
        {
            var list = new List<NetworkObject> { obj };
            instantiatedPrefabs.Add(prefabSource, list);
        }

        return obj;
    }


    // Called once runner.Despawn() is called
    void INetworkObjectProvider.ReleaseInstance(NetworkRunner runner, in NetworkObjectReleaseContext context)
    {
        context.Object.gameObject.SetActive(false);
    }

    public void RemoveNetworkObjectFromDict(NetworkObject obj)
    {
        if (instantiatedPrefabs.Count > 0)
        {
            foreach(var instanceList in instantiatedPrefabs.Values)
            {
                if (instanceList.Contains(obj))
                {
                    instanceList.Remove(obj);
                }
            }
        }
    }
}
