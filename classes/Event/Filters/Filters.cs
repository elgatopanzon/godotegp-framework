namespace GodotEGP.Event.Filters;

using Godot;
using System;

using GodotEGP.Event.Events;
using GodotEGP.Logging;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.ObjectPool;

public partial class EventFilter : IEventFilter
{
	public bool Enabled { get; set; } = true;
	public virtual bool Match(IEvent matchEvent)
	{
		return true;
	}

	public virtual void Init(params object[] p) 
	{
		Enabled = true;
	}
	public virtual void Reset()
	{
		Enabled = false;
	}
	public virtual void Dispose()
	{
		Reset();
	}
}

public partial class OwnerObjectFilter : EventFilter, IEventFilter
{
	private object _matchObject;

	public OwnerObjectFilter(object matchObject)
	{
		Init(matchObject);
	}

	public void Init(object matchObject)
	{
		_matchObject = matchObject;
	}
	public override void Init(params object[] p)
	{
		Init(p[0]);
		base.Init(p);
	}
	public override void Reset()
	{
		_matchObject = null;
		base.Reset();
	}

	public override bool Match(IEvent matchEvent)
	{
		// LoggerManager.LogDebug("match result", "", "result", matchEvent.Owner.Equals(_matchObject));
		return matchEvent.Owner.Equals(_matchObject);
	}
}

public partial class SignalTypeFilter : EventFilter, IEventFilter
{
	private string _matchSignal;

	public SignalTypeFilter(string matchSignal)
	{
		Init(matchSignal);
	}
	public void Init(string matchSignal)
	{
		_matchSignal = matchSignal;
	}
	public override void Init(params object[] p)
	{
		Init(p[0]);
		base.Init(p);
	}
	public override void Reset()
	{
		_matchSignal = null;
		base.Reset();
	}

	public override bool Match(IEvent matchEvent)
	{
		if (matchEvent.TryCast(out Events.GodotSignal e))
		{
			// LoggerManager.LogDebug("match result", "", "result", e.SignalName == _matchSignal);

			return e.SignalName == _matchSignal;
		}

		return false;
	}
}

public partial class ObjectTypeFilter : EventFilter, IEventFilter
{
	protected Type _matchType;

	public ObjectTypeFilter(Type matchType)
	{
		Init(matchType);
	}

	public void Init(Type matchType)
	{
		_matchType = matchType;
	}
	public override void Init(params object[] p)
	{
		Init(p[0]);
		base.Init(p);
	}
	public override void Reset()
	{
		_matchType = null;
		base.Reset();
	}

	public override bool Match(IEvent matchEvent)
	{
		return matchEvent.GetType().IsSubclassOf(_matchType) || matchEvent.GetType() == _matchType;
	}
}

public partial class OwnerObjectTypeFilter : ObjectTypeFilter, IEventFilter
{
	public OwnerObjectTypeFilter(Type matchType) : base(matchType)
	{
	}

	public override bool Match(IEvent matchEvent)
	{
		return (matchEvent.Owner.GetType().IsSubclassOf(_matchType) || matchEvent.Owner.GetType().Equals(_matchType));
	}
}

// inputmanager event filters
public partial class InputStateActionFilter : EventFilter, IEventFilter
{
	public enum State {
		Any = 0,
		Pressed = 1,
		JustPressed = 2,
		JustReleased = 3,
	}

	private StringName _action;
	private State _state;

	public InputStateActionFilter(StringName action, State state)
	{
		_action = action;
		_state = state;
	}

	public void Init(StringName action, State state)
	{
		_action = action;
		_state = state;
	}
	public override void Init(params object[] p)
	{
		Init(p[0], p[0]);
		base.Init(p);
	}
	public override void Reset()
	{
		_action = null;
		_state = State.Any;
		base.Reset();
	}

	public override bool Match(IEvent matchEvent)
	{
		if (matchEvent is InputStateChanged e)
		{
			if (e.ActionStates != null && e.ActionStates.ContainsKey(_action))
			{
				if (_state == State.Any)
				{
					return true;
				}
				if (_state == State.Pressed && e.ActionStates[_action].Pressed)
				{
					return true;
				}
				if (_state == State.JustPressed && e.ActionStates[_action].JustPressed)
				{
					return true;
				}
				if (_state == State.JustReleased && e.ActionStates[_action].JustReleased)
				{
					return true;
				}
			}
		}

		return false;
	}
}


public class EventFilterObjectPoolHandler : ObjectPoolHandler<EventFilter>
{
	public override EventFilter OnReturn(EventFilter instance)
	{
		instance.Reset();
		return instance;
	}
	public override EventFilter OnTake(EventFilter instance, params object[] p)
	{
		instance.Init(p);
		return instance;
	}
}
