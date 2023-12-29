using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class ResourceManager : MonoBehaviour
{
    #region Fields
    public Dictionary<string, UnityEngine.Object> _resources = new();

    #endregion

    #region Properties

    public bool LoadBase { get; set; }
    public bool LoadIntro { get; set; }
    public bool LoadGame { get; set; }
    public bool LoadLoading { get; set; }

    #endregion

    // ���ҽ� �񵿱� �ε� �޼���
    #region Asynchronous Loading

    /// <summary>
    /// �ݹ��� ó���ϴ� ���׸� �ڵ鷯�Դϴ�.
    /// </summary>
    private void AsyncHandlerCallback<T>(string key, AsyncOperationHandle<T> handle, Action<T> callback) where T : UnityEngine.Object
    {
        handle.Completed += operationHandle =>
        {
            if (!_resources.ContainsKey(key))
            {
                _resources.Add(key, operationHandle.Result);
            }
            else
            {
                Debug.LogWarning($"�ߺ��� ���ҽ� Ű: {key}. ���� ���ҽ��� ����մϴ�.");
            }

            callback?.Invoke(operationHandle.Result);
        };
    }

    private void AsyncHandlerAtlasCallback<T>(string key, AsyncOperationHandle<IList<T>> handle, Action<IList<T>> cb) where T : UnityEngine.Object
    {
        handle.Completed += operationHandle =>
        {
            IList<T> resultList = operationHandle.Result;

            foreach (var result in resultList)
            {
                string keyIndex = result.ToString().Split("_")[1].Split(" ")[0];
                string resourceKey = $"{key}[{keyIndex}]";

                if (!_resources.ContainsKey(resourceKey))
                {
                    _resources.Add(resourceKey, result);
                }
                else
                {
                    Debug.LogWarning($"�ߺ��� ���ҽ� Ű: {resourceKey}. ���� ���ҽ��� ����մϴ�.");
                }
            }
            cb?.Invoke(resultList);
        };
    }


    /// <summary>
    /// �񵿱� ������� ���ҽ��� �ε��ϰ� �ݹ��� ȣ���մϴ�.
    /// </summary>
    public void LoadAsync<T>(string key, Action<T> callback = null) where T : UnityEngine.Object
    {
        string loadKey = key;
        if (_resources.TryGetValue(loadKey, out UnityEngine.Object resource))
        {
            callback?.Invoke(resource as T);
            return;
        }

        if (key.Contains(".atlas"))
        {
            AsyncOperationHandle<IList<Sprite>> handle = Addressables.LoadAssetAsync<IList<Sprite>>(loadKey);
            AsyncHandlerAtlasCallback(key, handle, objs => callback?.Invoke(objs as T));
        }
        else if (loadKey.Contains(".sprite"))
        {
            loadKey = $"{key}[{key.Replace(".sprite", "")}]";
            AsyncOperationHandle<Sprite> handle = Addressables.LoadAssetAsync<Sprite>(loadKey);
            AsyncHandlerCallback(loadKey, handle, callback as Action<Sprite>);
        }
        else
        {
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(loadKey);
            AsyncHandlerCallback(loadKey, handle, callback);
        }
    }

    /// <summary>
    /// Ư�� �󺧿� ���� ��� ���ҽ��� �񵿱� ������� �ε��ϰ� �ݹ��� ȣ���մϴ�.
    /// </summary>
    public void AllLoadAsync<T>(string label, Action<string, int, int> callback) where T : UnityEngine.Object
    {
        var operation = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        operation.Completed += operationHandle =>
        {
            int loadCount = 0;
            int totalCount = operationHandle.Result.Count;
            foreach (var result in operationHandle.Result)
            {
                LoadAsync<T>(result.PrimaryKey, obj =>
                {
                    loadCount++;
                    callback?.Invoke(result.PrimaryKey, loadCount, totalCount);
                });
            }
        };
    }

    /// <summary>
    /// Ư�� �󺧿� ���� ��� ���ҽ��� �񵿱� ������� ��ε��մϴ�.
    /// </summary>
    public void UnloadAllAsync<T>(string label) where T : UnityEngine.Object
    {
        var operation = Addressables.LoadResourceLocationsAsync(label, typeof(T));
        operation.Completed += operationHandle =>
        {
            foreach (var result in operationHandle.Result)
            {
                if (!_resources.TryGetValue(result.PrimaryKey, out UnityEngine.Object resource)) continue;
                Addressables.Release(resource);
                _resources.Remove(result.PrimaryKey);
                Debug.Log($"{resource} ��ε�");
            }
        };
    }
    #endregion

    // ���ҽ� ���� �ε� �޼���
    #region Synchronous Loading

    /// <summary>
    /// ���ҽ��� ���������� �ε��մϴ�.
    /// </summary>
    public T Load<T>(string key) where T : UnityEngine.Object
    {
        if (_resources.TryGetValue(key, out UnityEngine.Object resource)) return resource as T;
        Debug.LogError($"Ű�� ã�� �� �����ϴ�. : {key}");
        return null;
    }
    #endregion

    // ������ �ν��Ͻ�ȭ �޼���
    #region Instantiation

    /// <summary>
    /// �������� �ν��Ͻ�ȭ�ϰ� ������ �ν��Ͻ��� ��ȯ�մϴ�.
    /// </summary>
    public GameObject InstantiatePrefab(string key, Transform transform = null)
    {
        GameObject resource = Load<GameObject>(key);

        GameObject instance = Instantiate(resource, transform);

        if (instance != null) return instance;
        Debug.LogError($"���ҽ��� �ν��Ͻ�ȭ���� ���߽��ϴ�.: { key}");
        return null;
    }
    #endregion
}

