namespace GodotEGP.Event;

using System;
using System.Collections.Generic;
using Godot;

using GodotEGP.Event.Events;
using GodotEGP.Event.Filter;


public partial class EventSubscription<T> : IEventSubscription<Event> where T : Event
{
    public object Subscriber { get; }
    public object CallbackMethod { get; }
    public bool IsHighPriority { get; }
    public bool Oneshot { get; set; }
    public Type EventType { get; }
    public List<IFilter> EventFilters { get; set; }
    public string Group { get; set; }

    public EventSubscription(object subscriberObj, Action<T> callbackMethod, bool isHighPriority = false, bool oneshot = false, List<IFilter> eventFilters = null, string groupName = "")
    {
        EventType = typeof(T);
        Subscriber = subscriberObj;
        CallbackMethod = callbackMethod;
        IsHighPriority = isHighPriority;
        Oneshot = oneshot;

        if (eventFilters == null)
        {
        	eventFilters = new List<IFilter>();
        }

        EventFilters = eventFilters;

        Group = groupName;
    }

    public void RunCallback(IEvent e)
    {
    	if (CallbackMethod is Action<T> cb)
    	{
    		cb((T) e);
    	}
    }
}
