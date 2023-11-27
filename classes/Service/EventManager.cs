namespace GodotEGP.Service;

using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

using GodotEGP.Logging;
using GodotEGP.Event;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filter;
using GodotEGP.Objects.Extensions;

public partial class EventManager : Service
{
	private Dictionary<Type, List<IEventSubscription<Event>>> _eventSubscriptions = new Dictionary<Type, List<IEventSubscription<Event>>>();
	private readonly object _eventSubscriptionsLock = new Object();

	private Dictionary<Type, EventQueue> _eventQueues = new Dictionary<Type, EventQueue>();
	private readonly object _eventQueuesLock = new Object();

	private Dictionary<GodotObject, string> _connectedSignals = new Dictionary<GodotObject, string>();
	private readonly object _connectedSignalsLock = new Object();

	public override void _Ready()
	{
		_SetServiceReady(true);
	}

	public void Subscribe(IEventSubscription<Event> eventSubscription)
	{
		lock (_eventSubscriptionsLock) {
			if (!_eventSubscriptions.TryGetValue(eventSubscription.EventType, out List<IEventSubscription<Event>> subList))
			{
				subList = new List<IEventSubscription<Event>>();
				_eventSubscriptions.TryAdd(eventSubscription.EventType, subList);

				LoggerManager.LogDebug("Creating subscriber list for event type", "", "eventType", eventSubscription.EventType.Name);
			}

			subList.Add(eventSubscription);

			LoggerManager.LogDebug("Adding event subscription", "", "eventSubscription", new Dictionary<string, object> 
					{
						{ "subscriberType", eventSubscription.Subscriber.GetType().Name },
						{ "eventType", eventSubscription.EventType.Name },
						{ "isHighPriority", eventSubscription.IsHighPriority },
						{ "filterCount", eventSubscription.EventFilters.Count },
					}
				);
		}
	}

	public bool Unsubscribe(IEventSubscription<Event> eventSubscription)
	{
		lock (_eventSubscriptionsLock) {
			if (_eventSubscriptions.TryGetValue(eventSubscription.EventType, out List<IEventSubscription<Event>> subList))
			{
				LoggerManager.LogDebug("Removing event subscription", "", "eventSubscription", new Dictionary<string, object> 
					{
						{ "subscriberType", eventSubscription.Subscriber.GetType().Name },
						{ "eventType", eventSubscription.EventType.Name },
						{ "isHighPriority", eventSubscription.IsHighPriority },
						{ "filterCount", eventSubscription.EventFilters.Count },
					}
				);

				return subList.Remove(eventSubscription);
			}
		}

		return false;
	}

	public void Unsubscribe(object eventSubscriptionOwner)
	{
		List<IEventSubscription<Event>> unsubs = new List<IEventSubscription<Event>>();

		foreach (var subList in _eventSubscriptions)
		{
			foreach (var sub in subList.Value)
			{
				if (sub.Subscriber.Equals(eventSubscriptionOwner))
				{
					unsubs.Add(sub);
				}
			}
		}

		foreach (var unsub in unsubs)
		{
			Unsubscribe(unsub);
		}
	}

	public bool Unsubscribe(string groupName)
	{
		bool unsubbed = false;

		foreach (var sub in GetSubscriptions(groupName))
		{
			unsubbed = Unsubscribe(sub);
		}	

		return unsubbed;
	}
		
	public List<IEventSubscription<Event>> GetSubscriptions(string groupName)
	{
		List<IEventSubscription<Event>> matchingSubs = new List<IEventSubscription<Event>>();

		foreach (var subList in _eventSubscriptions)
		{
			foreach (var sub in subList.Value)
			{
				if (sub.Group == groupName)
				{
					matchingSubs.Add(sub);
				}
			}
		}

		return matchingSubs;
	}

	public T GetQueue<T>() where T : EventQueue, new()
	{
		lock (_eventQueuesLock) {
			if (!_eventQueues.TryGetValue(typeof(T), out EventQueue eventQueue))
			{
				eventQueue = new T();
				_eventQueues.TryAdd(typeof(T), eventQueue);

				LoggerManager.LogDebug("Creating event queue", "", "eventQueue", typeof(T).Name);
			}

			return (T) eventQueue;
		}

	}

	public void Queue<T>(IEvent eventObj) where T : EventQueue, new()
	{
		GetQueue<T>().Queue(eventObj);
	}

	public Queue<IEvent> Fetch<T>(Type eventType, List<IFilter> eventFilters = null, int fetchCount = 1) where T : EventQueue, new()
	{
		// init eventFilters list if it's null
		lock (_eventQueuesLock) {
			if (Object.Equals(eventFilters, default(List<IFilter>)))
			{
				eventFilters = new List<IFilter>();
			}

			// add the eventType filter
			eventFilters.Add(new ObjectType(eventType));

			return GetQueue<T>().Fetch(eventFilters, fetchCount);
		}
	}

	public void Emit(IEvent eventObj)
	{
		lock (_eventQueuesLock) {
			bool eventConsumed = BroadcastEvent(eventObj, true);

			// queue event for low-priority subscribers
			GetQueue<EventQueueDeferred>().Queue(eventObj);
		}
	}

	public override void _Process(double delta)
	{
		// process events for each subscription type
		Queue<IEvent> eventQueue = GetQueue<EventQueueDeferred>().Fetch(null, 0);

		lock (_eventQueuesLock) {
			while (eventQueue.TryPeek(out IEvent eventObj))
			{
				// remove item from the queue
				eventObj = eventQueue.Dequeue();

				bool eventConsumed = BroadcastEvent(eventObj, false);

				LoggerManager.LogDebug("Deferred event consumed state", "", "event", new Dictionary<string, string> { { "eventType", eventObj.GetType().Name }, {"consumed", eventConsumed.ToString() } });

				// eventObj.ReturnInstance();
			}
		}
	}

	public bool BroadcastEvent(IEvent eventObj, bool broadcastHighPriority = false)
	{
		bool eventConsumed = false;

		if (eventObj == null)
		{
			LoggerManager.LogCritical("TODO: fix null event object");
		}

		// emit the event to high-priority subscribers
		if (_eventSubscriptions.TryGetValue(eventObj.GetType(), out List<IEventSubscription<Event>> subList))
		{
			foreach (IEventSubscription<Event> eventSubscription in subList.ToArray())
			{
				if (eventObj.GetType() == eventSubscription.EventType && eventSubscription.IsHighPriority == broadcastHighPriority)
				{

					bool filtersMatch = true;

					if (eventSubscription.EventFilters != null) 
					{
						foreach (IFilter eventFilter in eventSubscription.EventFilters)
						{
							filtersMatch = eventFilter.Match(eventObj);

							// stop validating if one of them fails
							if (!filtersMatch)
							{
								break;
							}
						}
					}

					if (filtersMatch)
					{
						LoggerManager.LogDebug($"Broadcasting {(broadcastHighPriority ? "high-priority" : "deferred")} event", "", "broadcast", new Dictionary<string, object> {{ "eventType", eventObj.GetType().Name }, { "subscriberType", eventSubscription.Subscriber.GetType().Name }, { "highPriority", broadcastHighPriority } });

						eventSubscription.RunCallback(eventObj);

						eventConsumed = true;

						if (eventSubscription.Oneshot)
						{
							Unsubscribe(eventSubscription);
						}
					}
				}
			}
		}
		
		return eventConsumed;
	}

	public void SubscribeSignal(GodotObject connectObject, string signalName, bool hasParams, IEventSubscription<Event> eventSubscription)
	{
		Action callback = () => __On_Signal(connectObject, signalName);
		Action<Variant> callbackParams = (p) => __On_Signal(connectObject, signalName, p);

		Callable cb;

		if (hasParams)
		{
			cb = Callable.From(callbackParams);
		}
		else
		{
			cb = Callable.From(callback);
		}

		if (_connectedSignals.TryAdd(connectObject, signalName))
		{
			connectObject.Connect(signalName, cb);

			LoggerManager.LogDebug("Connecting to godot signal", "", "signal", new Dictionary<string, string> { {"objectType", connectObject.GetType().Name}, {"signalName", signalName}  });
		}
		else
		{
			LoggerManager.LogWarning("Signal already connected", "", "signal", new Dictionary<string, string> { {"objectType", connectObject.GetType().Name}, {"signalName", signalName}  });

			return;
		}


		eventSubscription.EventFilters.Add(new OwnerObject(connectObject));
		eventSubscription.EventFilters.Add(new SignalType(signalName));

		Subscribe(eventSubscription);
	}

	public void __On_Signal(GodotObject connectObject, string signalName, params Variant[] signalParams)
	{
		connectObject.Emit<GodotSignal>((e) => e.SetSignalName(signalName).SetSignalParams(signalParams));
	}
	public void __On_Signal(GodotObject connectObject, string signalName, Variant signalParam)
	{
		connectObject.Emit<GodotSignal>((e) => e.SetSignalName(signalName).SetSignalParams(new Variant[] {signalParam}));
	}
}

public partial class EventQueueDeferred : EventQueue
{
	
}

