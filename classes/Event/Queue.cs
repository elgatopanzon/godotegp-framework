namespace GodotEGP.Event;

using Godot;
using System;
using System.Collections.Generic;

using GodotEGP.Logging;
using GodotEGP.Event.Events;
using GodotEGP.Event.Filter;

public partial class EventQueue
{
	private Queue<IEvent> _eventQueue = new Queue<IEvent>();

	public virtual void Queue(IEvent eventObj)
	{
		LoggerManager.LogDebug("Queuing event", this.GetType().Name.Replace(this.GetType().BaseType.Name, ""), "event", eventObj.ToString());

		_eventQueue.Enqueue(eventObj);
	}

	public Queue<IEvent> Fetch(List<IFilter> eventFilters = null, int fetchCount = 1)
	{
		Queue<IEvent> matchingQueue = new Queue<IEvent>();
		Queue<IEvent> nonMatchingQueue = new Queue<IEvent>();

		// validate against eventFilters and pull out matching Event objects
		while (_eventQueue.TryPeek(out IEvent eventItem))
		{
			eventItem = _eventQueue.Dequeue();

			bool filtersMatch = true;

			if (eventFilters != null) 
			{
				foreach (Filter.Filter eventFilter in eventFilters)
				{
					filtersMatch = eventFilter.Match(eventItem);

					// stop validating if one of them fails
					if (!filtersMatch)
					{
						break;
					}
				}
			}

			if (filtersMatch && (matchingQueue.Count < fetchCount || fetchCount == 0))
			{
				// put item in matching list
				matchingQueue.Enqueue(eventItem);		
			}
			else
			{
				// return item to new queue
				nonMatchingQueue.Enqueue(eventItem);
			}
		}

		// replace object's queue with newly created one
		_eventQueue = nonMatchingQueue;

		return matchingQueue;
	}
}
