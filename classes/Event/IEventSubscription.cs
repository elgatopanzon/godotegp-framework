namespace GodotEGP.Event;

using System;
using System.Collections.Generic;

using GodotEGP.Event.Events;
using GodotEGP.Event.Filters;

public partial interface IEventSubscription<in T> where T : Event
{
	object Subscriber { get; }
	object CallbackMethod { get; }
	Type EventType { get; }
	bool IsHighPriority { get; }
	bool Oneshot { get; }
	List<IEventFilter> EventFilters { get; set; }
	string Group { get; set; }
	void RunCallback(IEvent e);
}

