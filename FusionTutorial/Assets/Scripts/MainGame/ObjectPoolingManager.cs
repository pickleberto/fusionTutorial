using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class ObjectPoolingManager : MonoBehaviour, INetworkObjectPool
{
    // key: prefab; value: list of its instances
    private Dictionary<NetworkObject, List<NetworkObject>> instantiatedPrefabs = new();

    private void Start()
    {
        if(GlobalManagers.Instance != null)
        {
            GlobalManagers.Instance.ObjectPoolingManager = this;
        }
    }

    // Called once runner.Spawn() is called
    public NetworkObject AcquireInstance(NetworkRunner runner, NetworkPrefabInfo info)
    {
        NetworkObject networkObject = null;
        NetworkProjectConfig.Global.PrefabTable.TryGetPrefab(info.Prefab, out var prefab);
        instantiatedPrefabs.TryGetValue(prefab, out var networkObjects);

        bool foundMatch = false;
        if(networkObjects?.Count > 0)
        {
            foreach(var item in networkObjects)
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
            networkObject = CreateObjectInstance(prefab);
        }

        return networkObject;
    }

    private NetworkObject CreateObjectInstance(NetworkObject prefab)
    {
        var obj = Instantiate(prefab);

        if(instantiatedPrefabs.TryGetValue(prefab, out var instances))
        {
            instances.Add(obj);
        }
        else
        {
            var list = new List<NetworkObject> { obj };
            instantiatedPrefabs.Add(prefab, list);
        }

        return obj;
    }


    // Called once runner.Despawn() is called
    public void ReleaseInstance(NetworkRunner runner, NetworkObject instance, bool isSceneObject)
    {
        instance.gameObject.SetActive(false);
    }

    public void RemoveNetworkObjectFromDict(NetworkObject obj)
    {
        if (instantiatedPrefabs.Count > 0)
        {
            foreach(var instanceList in instantiatedPrefabs.Values)
            {
                if(instanceList.Contains(obj))
                {
                    instanceList.Remove(obj);
                }
            }
        }
    }
}
