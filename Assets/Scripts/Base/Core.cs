using System.Collections;
using UnityEngine;
using System.Threading.Tasks;

public class Core : MonoBehaviour
{
    public static Core Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(InitializeEngines());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //Последовательная инициализация
    private IEnumerator InitializeEngines()
     {
        SpawnEventsEngine();
        yield return new WaitUntil(() => EventsEngine.Instance != null);
        SpawnDataBaseEngine();
        yield return new WaitUntil(() => DataBaseEngine.Instance != null);
        SpawnWorldGeneratorEngine();
        yield return new WaitUntil(() => WorldGeneratorEngine.Instance != null);

        EventsEngine.Instance.Subscribe(DataBaseEngine.Instance);
        EventsEngine.Instance.Subscribe(WorldGeneratorEngine.Instance);
    }
    void SpawnEventsEngine()
    {
        if (EventsEngine.Instance == null)
        {
            EventsEngine.CreateInstance(gameObject);
        }
    }
    void SpawnDataBaseEngine()
    {
        if (DataBaseEngine.Instance == null)
        {
            DataBaseEngine.CreateInstance(gameObject);
        }
    }
    void SpawnWorldGeneratorEngine()
    {
        if (WorldGeneratorEngine.Instance == null)
        {
            WorldGeneratorEngine.CreateInstance(gameObject);
        }
    }
}
