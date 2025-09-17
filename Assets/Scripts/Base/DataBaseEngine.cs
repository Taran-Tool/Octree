using UnityEngine;
using System;

public class DataBaseEngine : Engine<DataBaseEngine>, IEventsSubscriber
{
    public event Action<string> OnDataSaved;
    protected override void Awake()
    {
        base.Awake();
    }

    //�������� � �������
    public override void SubscribeToEvents()
    {

    }
    public override void UnsubscribeFromEvents()
    {

    }
    //�������
    public void SaveData(string data)
    {
        OnDataSaved?.Invoke(data);
    }
}
