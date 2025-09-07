using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableObjectPool<T> : MonoBehaviour where T : Component
{
    public AddressableData addressableData;    
    private Dictionary<int, T> LoadedPrefabs = new();
    private Dictionary<int, Queue<T>> objectPool = new();

    // 이 함수를 바탕으로 CreateObject 함수 수정 예정
    private async Task<T> LoadAddressableAsset(int id, string key)
    {
        if(!LoadedPrefabs.ContainsKey(id))
        {
            AsyncOperationHandle<GameObject> handle = Addressables.InstantiateAsync(key, new Vector3(999,999,999), Quaternion.identity);
            await handle.Task; // handle의 완료를 기다림

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                // 인스턴스 생성 성공
                GameObject instantiatedObject = handle.Result;
                T loadedPrefab = instantiatedObject.GetComponent<T>();
                instantiatedObject.SetActive(false);
                if (loadedPrefab != null)
                {
                    LoadedPrefabs[id] = loadedPrefab;
                    return EnqueueObject(id, loadedPrefab);
                }
                else
                {
                    CappuDebug.LogError("Component not found on instantiated object.");
                    return null;
                }
            }
            else
            {
                CappuDebug.LogError("Failed to instantiate the addressable asset.");
                return null;
            }
        }

        return EnqueueObject(id, LoadedPrefabs[id]);
    }
    private T EnqueueObject(int id, T prefab)
    {
        if (!objectPool.ContainsKey(id))
        {
            objectPool[id] = new Queue<T>();
        }

        T pooledObject = Instantiate(prefab);
        objectPool[id].Enqueue(pooledObject);
        return pooledObject;
    }
    // 풀에서 오브젝트 획득
    protected async Task<T> GetObject(int id)
    {
        if (!addressableData.TryGetAddress(id, out string key))
        {
            CappuDebug.LogError("Failed to Get Address");
            return null;
        }

        if (!LoadedPrefabs.ContainsKey(id) || objectPool[id].Count == 0)
        {
            await LoadAddressableAsset(id, key);  // LoadedPrefab이 NULL이면 새로 생성 및 로드
        }

        return objectPool[id].Dequeue();
    }
    protected async Task<T> GetObjectInfo(int id)
    {
        if (!addressableData.TryGetAddress(id, out string key))
        {
            CappuDebug.LogError("Failed to Get Address");
            return null;
        }

        if (!LoadedPrefabs.ContainsKey(id) || objectPool[id].Count == 0)
        {
            await LoadAddressableAsset(id, key);  // LoadedPrefab이 NULL이면 새로 생성 및 로드
        }

        return objectPool[id].Peek();
    }

    // 오브젝트 반환
    protected void ReturnObject(int id, T obj)
    {
        if (!objectPool.ContainsKey(id))
        {
            objectPool[id] = new Queue<T>();
        }
        objectPool[id].Enqueue(obj);
    }
}