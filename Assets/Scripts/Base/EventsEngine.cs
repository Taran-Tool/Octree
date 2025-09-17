using UnityEngine;

public class EventsEngine : Engine<EventsEngine>, IEventsSubscriber
{
    protected override void Awake()
    {
        base.Awake();
    }

    public void Subscribe(IEventsSubscriber subscriber)
    {
        subscriber.SubscribeToEvents();
    }

    public void Unsubscribe(IEventsSubscriber subscriber)
    {
        subscriber.UnsubscribeFromEvents();
    }
    //Подписки и отписки
    public override void SubscribeToEvents()
    {

    }
    public override void UnsubscribeFromEvents()
    {

    }
}
