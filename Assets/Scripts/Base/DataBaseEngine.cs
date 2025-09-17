using UnityEngine;
using System;

public class DataBaseEngine : Engine<DataBaseEngine>, IEventsSubscriber
{
    public event Action<string> OnDataSaved;
    protected override void Awake()
    {
        base.Awake();
    }

    //Подписки и отписки
    public override void SubscribeToEvents()
    {

    }
    public override void UnsubscribeFromEvents()
    {

    }
    //События
    public void SaveData(string data)
    {
        OnDataSaved?.Invoke(data);
    }
}
