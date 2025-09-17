using UnityEngine;
using System;

public class WorldGeneratorEngine : Engine<WorldGeneratorEngine>, IEventsSubscriber
{
    public static readonly int chunksSize = 32;
    private GameObject currentWorld;

    protected override void Awake()
    {
        base.Awake();
    }

    public GameObject GetCurrentWorld()
    {
        return currentWorld;
    }

    public void CreateNewWorld(string name)
    {
        //запрашиваю у бд перечень ранее созданных миров
        int number = GetWorldsLastNumber() == 0 ? 0 : GetWorldsLastNumber() + 1;
        InstanceDescr worldDescr = new InstanceDescr(number, name);

        GameObject worldGO = new GameObject(name) { transform = {parent = gameObject.transform } };
        World world = worldGO.AddComponent<World>();
        world.Initialize(worldDescr);
        currentWorld = world.GetWorldInstance();
        //создание домашней локации
        currentWorld.GetComponent<World>().CreateLocation(new HomeLocation(), (int) Location.locationSizes.big);
    }

    public int GetWorldsLastNumber()
    {
        // return DataBase.instance.GetLastID("Worlds");
        return 1;
    }

    //Подписки и отписки
    public override void SubscribeToEvents()
    {   
        //DataBaseEngine
        if (DataBaseEngine.Instance != null)
        {
            DataBaseEngine.Instance.OnDataSaved += Test;
        }        
    }
    public override void UnsubscribeFromEvents()
    {
        if (DataBaseEngine.Instance != null)
        {
            DataBaseEngine.Instance.OnDataSaved -= Test;
        }
    }
    //События
    void Test(string data)
    {
        Debug.Log("WG got that DB just aved data! Data is: " + data);
    }

}
