using UnityEngine;

public abstract class Engine<T> : MonoBehaviour where T : Engine<T>, IEventsSubscriber
{
    public static T Instance;

    public static T CreateInstance(GameObject go)
    {
        if (Instance == null)
        {
            GameObject obj = new GameObject(typeof(T).Name);
            obj.transform.parent = go.transform;
            Instance = obj.AddComponent<T>();
        }
        return Instance;
    }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
        }
        else
        {
            if (Instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    public abstract void SubscribeToEvents();
    public abstract void UnsubscribeFromEvents();
}
