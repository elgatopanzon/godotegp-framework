namespace GodotEGP.State;

using Godot;
using System;

using System.Collections.Generic;
using GodotEGP.Logging;

public partial class StateMachine
{
	// holds object which owns the state
	object _ownerObject;

	// holds list of states
	Dictionary<object, Dictionary<CallbackType, Action<object, object>>> _states;

	// current state stack and accessor
	private Stack<object> _state = new Stack<object>();

	public object State
	{
		get { 
			return _state.Peek(); 
		}
		set { 
			_state.Clear();
			_state.Push(value);
		}
	}

	// enum for valid state change callbacks
	public enum CallbackType
	{
		OnEnter, // pre-changed
		OnChanged, // actually changed
		OnExit // pre-exit
	}

	public StateMachine(object ownerObject)
	{
		_ownerObject = ownerObject;

		_states = new Dictionary<object, Dictionary<CallbackType, Action<object, object>>>();

		LoggerManager.LogDebug("Creating new instance for object", _ownerObject.GetType().Name);
	}

	public bool Add(object stateName)
	{
		// try to add the dictionay for the added state
		// will fail if state with this key name already exists
		bool res = _states.TryAdd(stateName, new Dictionary<CallbackType, Action<object, object>>());

		if (res)
		{
			LoggerManager.LogDebug("Adding state", _ownerObject.GetType().Name, "state", stateName);
		}
		else
		{
			LoggerManager.LogError("Adding state failed", _ownerObject.GetType().Name, "state", stateName);
		}

		return res;
	}

	public void Init(object stateName)
	{
		SetState(stateName);
		Change(stateName);
	}

	public void SetState(object stateName)
	{
		if (!IsValidState(stateName))
		{
			throw new InvalidStateException(stateName);
		}

		State = stateName;
	}

	public bool IsValidState(object stateName)
	{
		return _states.ContainsKey(stateName);
	}

	public void Change(object newState)
	{
		// run state change callbacks
		if (IsValidState(newState))
		{
			_onStateChanged(State, newState);

			SetState(newState);
		}
	}

	private void _onStateChanged(object prevState, object newState)
	{
		LoggerManager.LogDebug("Changing state", _ownerObject.GetType().Name, "stateChange", $"{prevState} => {newState}");

		_runCallback(prevState, CallbackType.OnExit, prevState, newState);
		_runCallback(newState, CallbackType.OnEnter, prevState, newState);


		// run final changed callback
		_runCallback(newState, CallbackType.OnChanged, prevState, newState);
	}

	public void Push(object pushState)
	{
		LoggerManager.LogDebug("Pushing state to stack", _ownerObject.GetType().Name, "state", pushState);

		_onStateChanged(State, pushState);

		_state.Push(pushState);
	}

	public object Pop()
	{
		if (_state.Count > 1)
		{
			object poppedState = _state.Pop();

			LoggerManager.LogDebug("Popped state from stack", _ownerObject.GetType().Name, "state", poppedState);

			_onStateChanged(poppedState, State);

			return poppedState;
		}

		return null;
	}

	public void _runCallback(object currentState, CallbackType callbackType, object prevState, object newState)
	{
		if (_states[currentState].ContainsKey(callbackType))
		{
			LoggerManager.LogDebug("Running state change callback", _ownerObject.GetType().Name, "callback", $"{currentState} => {callbackType.ToString()}");
			_states[currentState][callbackType](prevState, newState);
		}
	}

	public void RegisterCallback(object stateName, CallbackType callbackType, Action<object, object> callbackFunc)
	{
		if (IsValidState(stateName))
		{
			_states[stateName].TryAdd(callbackType, callbackFunc);
		}
	}
}

public partial class InvalidStateException : Exception
{
    public InvalidStateException()
    {
    }

    public InvalidStateException(object message)
        : base(message.ToString())
    {
    }

    public InvalidStateException(object message, Exception inner)
        : base(message.ToString(), inner)
    {
    }
}
