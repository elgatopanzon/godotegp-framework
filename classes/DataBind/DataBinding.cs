/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : DataBinding
 * @created     : Monday Nov 13, 2023 15:26:23 CST
 */

namespace GodotEGP.DataBind;

using System;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Event;
using GodotEGP.Config;

public abstract partial class DataBinding : Node
{

}

public partial class DataBinding<T, TEvent> : DataBinding where TEvent : Event
{
	Action<T> _setterFirstCb;
	Func<T> _getterFirstCb;

	object _objectFirst;

	EventSubscription<TEvent> _eventSub;

	public EventSubscription<TEvent> EventSubscription {
		get {
			return _eventSub;
		}
		set {
			_eventSub = value;
		}
	}

	public object ObjectFirst {
		get {
			return _objectFirst;
		}
	}


	public DataBinding(object objectFirst, Func<T> getterFirstCb, Action<T> setterFirstCb, bool initialSet = true)
	{
		_objectFirst = objectFirst;
		
		_setterFirstCb = setterFirstCb;
		_getterFirstCb = getterFirstCb;

		_eventSub = objectFirst.SubscribeOwner<TEvent>(_On_ObjectFirst_Changed, isHighPriority: true);

		// trigger initial binding
		if (initialSet)
		{
			_On_ObjectFirst_Changed(null);
		}
	}

	public void Destroy()
	{
		ServiceRegistry.Get<EventManager>().Unsubscribe(_objectFirst);
		this.QueueFree();
	}

	public override void _Ready()
	{
		// AddChild(_bindTimer);
	}

	public void _On_ObjectFirst_Changed(IEvent e)
	{
		try
		{
			var v = _getterFirstCb();

			LoggerManager.LogDebug("Object first changed", "", "val", v);

			_setterFirstCb(v);	
		}
		catch (ObjectDisposedException)
		{
			if (!IsQueuedForDeletion())
			{
				Destroy();
			}
		}
	}
}
