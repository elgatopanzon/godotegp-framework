namespace GodotEGP.Event;

using System;
using System.Collections.Generic;
using Godot;

using GodotEGP.Event.Events;
using GodotEGP.Event.Filters;
using GodotEGP.Objects.ObjectPool;

public partial class EventSubscription<T> : IPoolableObject, IEventSubscription<Event> where T : Event
{
    public object Subscriber { get; private set; }
    public object CallbackMethod { get; private set; }
    public bool IsHighPriority { get; private set; }
    public bool Oneshot { get; set; }
    public Type EventType { get; private set; }
    public List<IEventFilter> EventFilters { get; set; }
    public string Group { get; set; }

    public EventSubscription(object subscriberObj, Action<T> callbackMethod, bool isHighPriority = false, bool oneshot = false, List<IEventFilter> eventFilters = null, string groupName = "")
    {
    	Init(subscriberObj, callbackMethod, isHighPriority, oneshot, eventFilters, groupName);
    }

    public void RunCallback(IEvent e)
    {
    	if (CallbackMethod is Action<T> cb)
    	{
    		cb((T) e);
    	}
    }

	public void Init(object subscriberObj, Action<T> callbackMethod, bool isHighPriority = false, bool oneshot = false, List<IEventFilter> eventFilters = null, string groupName = "")
	{
        EventType = typeof(T);
        Subscriber = subscriberObj;
        CallbackMethod = callbackMethod;
        IsHighPriority = isHighPriority;
        Oneshot = oneshot;

        if (eventFilters == null)
        {
        	eventFilters = new List<IEventFilter>();
        }

        EventFilters = eventFilters;

        Group = groupName;
		
	}
    public void Init(params object[] p)
    {
    	Init(p[0], p[1],
    		(bool) ((p.Length >= 2) ? p[2] : null),
    		(bool) ((p.Length >= 3) ? p[3] : null),
    		(List<IEventFilter>) ((p.Length >= 4) ? p[4] : null),
    		(string) ((p.Length >= 5) ? p[5] : null)
    		);
    }
    public void Reset()
    {
    	Subscriber = null;
    	CallbackMethod = null;
    	IsHighPriority = false;
    	Oneshot = false;
    	EventType = null;

    	Group = null;

    	// TODO: return filter objects to pool
    	EventFilters = null;
    }
}
