namespace GodotEGP.Event.Filter;

using Godot;
using System;

using GodotEGP.Event.Events;
using GodotEGP.Logging;
using GodotEGP.Objects.Extensions;

public partial class Filter : IFilter
{
	public bool Match(IEvent matchEvent)
	{
		return true;
	}
}

public partial class OwnerObject : IFilter
{
	private object _matchObject;

	public OwnerObject(object matchObject)
	{
		_matchObject = matchObject;
	}

	public bool Match(IEvent matchEvent)
	{
		// LoggerManager.LogDebug("match result", "", "result", matchEvent.Owner.Equals(_matchObject));
		return matchEvent.Owner.Equals(_matchObject);
	}
}

public partial class SignalType : IFilter
{
	private string _matchSignal;

	public SignalType(string matchSignal)
	{
		_matchSignal = matchSignal;
	}

	public bool Match(IEvent matchEvent)
	{
		if (matchEvent.TryCast(out Events.GodotSignal e))
		{
			// LoggerManager.LogDebug("match result", "", "result", e.SignalName == _matchSignal);

			return e.SignalName == _matchSignal;
		}

		return false;
	}
}

public partial class ObjectType : IFilter
{
	private Type _matchType;

	public ObjectType(Type matchType)
	{
		_matchType = matchType;
	}

	public bool Match(IEvent matchEvent)
	{
		return matchEvent.GetType().IsSubclassOf(_matchType);
	}
}

public partial class OwnerObjectType : IFilter
{
	private Type _matchType;

	public OwnerObjectType(Type matchType)
	{
		_matchType = matchType;
	}

	public bool Match(IEvent matchEvent)
	{
		return (matchEvent.Owner.GetType().IsSubclassOf(_matchType) || matchEvent.Owner.GetType().Equals(_matchType));
	}
}

// inputmanager event filters
public partial class InputStateAction : IFilter
{
	public enum State {
		Any = 0,
		Pressed = 1,
		JustPressed = 2,
		JustReleased = 3,
	}

	private StringName _action;
	private State _state;

	public InputStateAction(StringName action, State state)
	{
		_action = action;
		_state = state;
	}

	public InputStateAction(StringName action)
	{
		_action = action;
	}

	public bool Match(IEvent matchEvent)
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
