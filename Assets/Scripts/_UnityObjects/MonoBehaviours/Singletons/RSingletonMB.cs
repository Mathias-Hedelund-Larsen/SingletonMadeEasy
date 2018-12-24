﻿using UnityEngine;

/// <summary>
/// Base class for resource creating monobehaviour singletons, which other classes can derive from to become singletons.
/// You should mark your singleton (the derived class) as sealed so you can't derive from it.
/// </summary>
/// <typeparam name="T">This generic type, needs to be the type of the derived class</typeparam>
public abstract class RSingletonMB<T> : MonoBehaviour, ISingleton where T : RSingletonMB<T>
{
    //The singleton instance field
    private static T _instance;

    //Object to achieve a lock from
    private static readonly object _lockObject = new object();

    //Whether if the singleton has been destroyed should only happen when the game closes
    protected static bool _destroyed = false;

    /// <summary>
    /// Get accesor for the singleton Instance
    /// </summary>
    public static T Instance
    {
        get
        {
            //Locking for thread safe
            lock (_lockObject)
            {
                if (_destroyed)
                {
                    return null;
                }

                if (_instance == null)
                {
                    _instance = Instantiate(SingletonManager.Instance.GetAsset<T>());
                    _instance.OnInstantiated();
                    DontDestroyOnLoad(_instance);
                    SingletonManager.Instance.AddInstance(_instance);
                }

                return _instance;
            }
        }
    }

    /// <summary>
    /// Implementation of the ISingleton called by the Instance get right after Awake and before start use this for initialization
    /// </summary>
    public abstract void OnInstantiated();

#if UNITY_EDITOR || DEVELOPMENT_BUILD

    /// <summary>
    /// Used to check if you have set the singleton up right, don't use start for initialization use OnInstantiated
    /// </summary>
    private void Start()
    {
        if (_instance == null || _instance.GetInstanceID() != GetInstanceID())
        {
            Destroy(gameObject);

            Debug.LogError("The instance has not been set before start or has a different id, this indicates that the singleton is coming from a scene");
        }
    }

#endif

    /// <summary>
    /// The ondestroy call made by unity you can override this but remember to base for the _destroyed bool to be set
    /// </summary>
    protected virtual void OnDestroy()
    {
        _destroyed = true;
    }

#if UNITY_EDITOR

    private void Reset()
    {
        var gobs = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().GetRootGameObjects();

        for (int i = 0; i < gobs.Length; i++)
        {
            if (gobs[i] == gameObject)
            {
                Debug.LogError("The resource singletons should be in the Resources Singletons folder please move: " + gameObject.name + " to the folder");
            }

            foreach (Transform item in gobs[i].transform)
            {
                if (item.gameObject == gameObject)
                {
                    Debug.LogError("The resource singletons should be in the Resources Singletons folder please move: " + gameObject.name + " to the folder");
                }
            }
        }

        if (GetComponentsInChildren<ISingleton>().Length > 1)
        {
            DestructionWindow.OpenWindow("There shouldn't be multiple singletons in a hierarchy on a resource singleton object, it will now be destroyed on: ", this);
        }
    }

#endif
}