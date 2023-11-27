namespace GodotEGP.State;

using Godot;
using System;

using System.Collections.Generic;
using GodotEGP.Logging;

public partial class HStateMachine
{
	// owner object of this instance
	private Object Owner;

	// parent state owner object
	private HStateMachine Parent;

	// callback actions for this state
	public Action OnEnter;
	public Action OnUpdate;
	public Action OnExit;

	private Dictionary<Type, HStateMachine> SubStates;
	private Dictionary<int, HStateMachine> Transitions;
	private HStateMachine DefaultSubState;
	public HStateMachine CurrentSubState;

	public HStateMachine(object owner = null)
	{
		SubStates = new Dictionary<Type, HStateMachine>();
		Transitions = new Dictionary<int, HStateMachine>();

		// set callbacks to own methods
		OnEnter = CallbackOnEnter;
		OnUpdate = CallbackOnUpdate;
		OnExit = CallbackOnExit;

		Owner = owner;
	}

	public void AddState(HStateMachine subState)
	{
		// set parent and owner references
		subState.Parent = this;
		subState.Owner = Owner;

		// add the sub state
		if (SubStates.TryAdd(subState.GetType(), subState))
		{
			LoggerManager.LogDebug("Adding sub-state", this.GetType().Name, "state", subState.GetType().Name);

			// if there's no sub states, set this state as the default state
			if (SubStates.Count == 1)
			{
				LoggerManager.LogDebug("Setting default sub-state", this.GetType().Name, "state", subState.GetType().Name);

				DefaultSubState = subState;
			}
		}
		else
		{
			throw new StateExistsException($"Sub state {subState.GetType().Name} already exists");
		}
	}

	public void AddTransition(HStateMachine from, HStateMachine to, int trigger)
	{
		from.Transitions.TryAdd(trigger, to);
	}

	public void Enter()
	{
		OnEnter();

		// assign currentSubState to default if current isn't set
		if (CurrentSubState == null && DefaultSubState != null)
		{
			CurrentSubState = DefaultSubState;
		}

		CurrentSubState?.Enter();
	}

	public void Update()
	{
		OnUpdate();
		CurrentSubState?.Update();
	}

	public void Exit()
	{
		CurrentSubState?.Exit();
		OnExit();
	}

	public virtual void CallbackOnEnter() { }
	public virtual void CallbackOnUpdate() { }
	public virtual void CallbackOnExit() { }

	public void Change(HStateMachine state, bool runUpdate = false)
	{
		// run Exit callback on current sub state
		CurrentSubState?.Exit();

		// change current subState
		if (SubStates.ContainsValue(state))
		{
			LoggerManager.LogDebug($"Changing state {CurrentSubState.GetType().Name} => {state.GetType().Name}", this.GetType().Name);
			CurrentSubState = state;
		}
		else
		{
			throw new InvalidChangeStateException($"{state.GetType()} is not a valid sub state of {this.GetType()}");
		}

		// run Enter callback on new sub state
		CurrentSubState.Enter();

		// run a single process iteration right away
		if (runUpdate)
		{
			CurrentSubState.Update();
		}
	}

	public void Transition(int trigger, bool runUpdate = false)
	{
		var root = this;
		while (root?.Parent != null)
		{
			root = root.Parent;
		}

		while (root != null)
		{
			if (root.Transitions.TryGetValue(trigger, out HStateMachine to))
			{
				root.Parent?.Change(to, runUpdate);
				return;
			}

			root = root.CurrentSubState;
		}

		throw new UnconsumedTransitionException($"Transition {trigger} in state {CurrentSubState.GetType().Name} was not consumed by any transition");
	}
}

public partial class UnconsumedTransitionException : Exception
{
    public UnconsumedTransitionException(string message) : base(message) { }
}

public partial class InvalidChangeStateException : Exception
{
    public InvalidChangeStateException(string message) : base(message) { }
}

public partial class StateExistsException : Exception
{
    public StateExistsException(string message) : base(message) { }
}
