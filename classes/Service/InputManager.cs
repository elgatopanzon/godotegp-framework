/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : InputManager
 * @created     : Wednesday Nov 22, 2023 15:42:37 CST
 */

namespace GodotEGP.Service;

using System;
using System.Collections.Generic;
using System.Linq;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.State;

public partial class InputManager : Service
{
	// states
	class InputState : HStateMachine {}
	class InputConfigurationState : HStateMachine {}
	class InputListeningState : HStateMachine {}
	class InputProcessingState : HStateMachine {}

	private InputState _inputState  { get; set; }
	private InputConfigurationState _inputConfigurationState  { get; set; }
	private InputListeningState _inputListeningState  { get; set; }
	private InputProcessingState _inputProcessingState  { get; set; }

	private const int CONFIGURATION_STATE = 0;
	private const int LISTENING_STATE = 1;
	private const int PROCESSING_STATE = 2;

	// config
	private InputManagerConfig _config { get; set; }
	private InputMappingConfig _mappingConfig { get; set; }

	private bool _processingInputEvent;
	private bool _triggerConfiguration = true;

	// input state
	private Dictionary<StringName, ActionInputState> _actionStates = new();
	private MouseState _mouseState = new();

	public Dictionary<StringName, ActionInputState> ActionStates {
		get { return _actionStates; }
	}
	public MouseState MouseState {
		get { return _mouseState; }
	}

	private Dictionary<string, JoypadState> _joypadStates = new();
	private Dictionary<int, string> _joypadGuidMappings = new();

	public Dictionary<string, JoypadState> JoypadStates {
		get { return _joypadStates; }
	}

	private Godot.Collections.Array<int> _connectedJoypadCount = Input.GetConnectedJoypads();

	private bool _emitJoypadStateEvents = false;

	public InputManager()
	{
		// init default configs
		_config = new();
		_mappingConfig = new();

		// setup states
		_inputState = new();
		_inputConfigurationState = new();
		_inputListeningState = new();
		_inputProcessingState = new();

		_inputState.AddState(_inputConfigurationState);
		_inputState.AddState(_inputListeningState);
		_inputState.AddState(_inputProcessingState);

		_inputConfigurationState.OnEnter = _State_Configuration_OnEnter;
		_inputConfigurationState.OnUpdate = _State_Configuration_OnUpdate;
		_inputListeningState.OnEnter = _State_Listening_OnEnter;
		_inputListeningState.OnUpdate = _State_Listening_OnUpdate;
		_inputProcessingState.OnEnter = _State_Processing_OnEnter;
		_inputProcessingState.OnUpdate = _State_Processing_OnUpdate;

		_inputState.AddTransition(_inputConfigurationState, _inputListeningState, LISTENING_STATE);
		_inputState.AddTransition(_inputProcessingState, _inputListeningState, LISTENING_STATE);

		_inputState.AddTransition(_inputListeningState, _inputProcessingState, PROCESSING_STATE);

		_inputState.AddTransition(_inputListeningState, _inputConfigurationState, CONFIGURATION_STATE);
		_inputState.AddTransition(_inputProcessingState, _inputConfigurationState, CONFIGURATION_STATE);
	}

	public void SetMappingConfig(InputMappingConfig mappingConfig)
	{
		_mappingConfig = mappingConfig;
	}

	public void SetConfig(InputManagerConfig config)
	{
		LoggerManager.LogDebug("Setting config");

		_config = config;

		if (!GetReady())
		{
			_SetServiceReady(true);
		}

		_inputState.Transition(CONFIGURATION_STATE);
	}


	/*******************
	*  Godot methods  *
	*******************/

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_inputState.Enter();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_inputState.Update();
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		_On_InputEvent(@event);
	}

	/*********************
	*  Service methods  *
	*********************/
	
	// Called when service is registered in manager
	public override void _OnServiceRegistered()
	{
	}

	// Called when service is deregistered from manager
	public override void _OnServiceDeregistered()
	{
		// LoggerManager.LogDebug($"Service deregistered!", "", "service", this.GetType().Name);
	}

	// Called when service is considered ready
	public override void _OnServiceReady()
	{
	}

	/***************************
	*  State related methods  *
	***************************/

	public void _State_Configuration_OnEnter()
	{
		LoggerManager.LogDebug("Entering configuration state");

		ResetInputActions();
		SetupInputActions();

		UpdateInputState();

		_inputState.Transition(LISTENING_STATE);
	}
	
	public void _State_Configuration_OnUpdate()
	{
	}

	public void _State_Listening_OnEnter()
	{
		LoggerManager.LogDebug("Entering listening state");
	}
	
	public void _State_Listening_OnUpdate()
	{
		var connectedJoypads = Input.GetConnectedJoypads();

		// we don't want to emit an event here, we just want to update the state
		if (Input.IsAnythingPressed() && _processingInputEvent == false)
		{
			LoggerManager.LogDebug("Something pressed, updating input state");
			UpdateInputState();
		}

		if (connectedJoypads.Count != _connectedJoypadCount.Count)
		{
			LoggerManager.LogDebug("Connected joypad count changed");
			UpdateInputState();
			_triggerConfiguration = true;
		}

		if (_triggerConfiguration)
		{
			LoggerManager.LogDebug("Triggered action re-configuration");
			_inputState.Transition(CONFIGURATION_STATE);

			_triggerConfiguration = false;
		}

		_processingInputEvent = false;
		_connectedJoypadCount = connectedJoypads;
	}

	public void _State_Processing_OnEnter()
	{
		LoggerManager.LogDebug("Entering processing state");
	}

	public void _State_Processing_OnUpdate()
	{
		LoggerManager.LogDebug("Updating processing state");

		UpdateInputState();

		LoggerManager.LogDebug("Action states", "", "state", _actionStates);
		LoggerManager.LogDebug("Mouse state", "", "state", _mouseState);
		LoggerManager.LogDebug("Joypad states", "", "state", _joypadStates);

		this.Emit<InputStateChanged>(e => e.SetStates(_actionStates, _joypadStates, _mouseState));

		// return to listening state
		_inputState.Transition(LISTENING_STATE);
	}

	/*************************************
	*  Input action management methods  *
	*************************************/

	public void UpdateInputState()
	{
		LoggerManager.LogDebug("Updating input state");

		UpdateMouseState();
		UpdateJoypadState();

		UpdateActionInputStates();

		LoggerManager.LogDebug("Updating input state finished");
	}
	
	public void ResetInputActions(bool eraseActions = true)
	{
		foreach (var action in InputMap.GetActions())
		{
			if (ActionExists(action))
			{
				ResetInputAction(action);

				// reset events for + and - when the action is a joystick
				if (_config.Actions[action].ControlType == InputActionType.Joystick)
				{
					ResetInputAction(action+"-");
					ResetInputAction(action+"+");
				}

				if (eraseActions)
				{
					LoggerManager.LogDebug("Erasing input action", "", "action", action.ToString());

					InputMap.EraseAction(action);

					_actionStates.Remove(action);

					// erase actions for + and - when the action is a joystick
					if (_config.Actions[action].ControlType == InputActionType.Joystick)
					{
						InputMap.EraseAction(action+"-");
						InputMap.EraseAction(action+"+");

						_actionStates.Remove(action+"-");
						_actionStates.Remove(action+"+");
					}
				}
			}

		}
	}

	public void ResetInputAction(StringName action)
	{
		LoggerManager.LogDebug("Resetting input action", "", "action", action.ToString());
		
		foreach (var e in InputMap.ActionGetEvents(action))
		{
			LoggerManager.LogDebug("Erasing action event", "", action.ToString(), e);
			InputMap.ActionEraseEvent(action, e);
		}
	}

	public bool ActionExists(StringName action)
	{
		return _mappingConfig.Mappings.ContainsKey(action.ToString());
	}

	public void SetupInputActions()
	{
		// add actions from config known actions
		foreach (var action in _config.Actions)
		{
			LoggerManager.LogDebug("Adding input action", "", action.Key, action.Value);
			InputMap.AddAction(action.Key, (float) action.Value.Deadzone);

			// if the action is a joystick, create actions for both + and -
			if (action.Value.ControlType == InputActionType.Joystick)
			{
				InputMap.AddAction(action.Key+"-", (float) action.Value.Deadzone);
				InputMap.AddAction(action.Key+"+", (float) action.Value.Deadzone);
			}

			ConfigureActionMapping(action.Key);

			// add current action to input states
			if (!_actionStates.ContainsKey(action.Key))
			{
				_actionStates.Add(action.Key, new ActionInputState());
				_actionStates[action.Key].Config = action.Value;
			}
		}
	}

	public void ConfigureActionMapping(StringName action)
	{
		if (_mappingConfig.Mappings.TryGetValue(action, out var ev))
		{
			foreach (var actionMapping in ev.Events)
			{
				LoggerManager.LogDebug("Action mapping found", "", "mapping", actionMapping);

				InputEvent e = actionMapping.ToInputEvent();

				// set correct device from the mapping config
				if (actionMapping.IsJoypadMapping)
				{
					int joyId = 0;

					if (_connectedJoypadCount.Count == 0)
					{
						LoggerManager.LogWarning("No joypads, cannot map action!", "", "action", action.ToString());
						continue;
					}

					// set joy id to the found guid
					if (actionMapping.DeviceGuid.Length == 0)
					{
						if (_config.Actions[action].Player == 0)
						{
							LoggerManager.LogDebug("Empty guid for primary action mapping", "", action, e);

							joyId = _joypadGuidMappings.FirstOrDefault().Key;

							LoggerManager.LogDebug("Using first device id", "", action, joyId);
						}
						else
						{
							LoggerManager.LogDebug("Action is not primary, don't auto-bind guid", "", "playerSlot", _config.Actions[action].Player);
						}
					}
					else
					{
						joyId = _joypadGuidMappings.FirstOrDefault(x => x.Value == actionMapping.DeviceGuid).Key;

						if (_joypadGuidMappings.ContainsKey(joyId) && _joypadGuidMappings[joyId] == actionMapping.DeviceGuid)
						{
							LoggerManager.LogDebug("Found device id from guid", "", actionMapping.DeviceGuid, joyId);
						}
						else
						{
							joyId = _joypadGuidMappings.FirstOrDefault().Key;
							LoggerManager.LogDebug("Guid not found, using first device id", "", _joypadGuidMappings[joyId], joyId);
						}

					}

					e.Device = joyId;
				}

				// add extra events for the + and - directions of this action
				if (e is InputEventJoypadMotion ej && _config.Actions[action].ControlType == InputActionType.Joystick)
				{
					LoggerManager.LogDebug("Adding + and - axis events", "", "action", action.ToString());

					InputEvent ejPlusMinus = actionMapping.ToInputEvent();
					ejPlusMinus.Device = e.Device;

					if (ejPlusMinus is InputEventJoypadMotion ejMinus)
					{
						ejMinus.AxisValue = -1;
						InputMap.ActionAddEvent(action+"-", ejMinus);
					}

					ejPlusMinus = actionMapping.ToInputEvent();
					ejPlusMinus.Device = e.Device;

					if (ejPlusMinus is InputEventJoypadMotion ejPlus)
					{
						ejPlus.AxisValue = 1;
						InputMap.ActionAddEvent(action+"+", ejPlus);
					}
				}
				else
					InputMap.ActionAddEvent(action, e);
			}
		}
	}

	public void UpdateActionInputStates()
	{
		// update internal state for all known actions
		foreach (var action in _config.Actions)
		{
			var actionName = action.Key;
			var actionConfig = action.Value;

			_actionStates[actionName].Pressed = Input.IsActionPressed(actionName);
			_actionStates[actionName].JustPressed = Input.IsActionJustPressed(actionName);
			_actionStates[actionName].JustReleased = Input.IsActionJustReleased(actionName);

			// set the strength value if it's an axis
			if (actionConfig.ControlType == InputActionType.Joystick || actionConfig.ControlType == InputActionType.Trigger)
			{
				_emitJoypadStateEvents = true;

				if (actionConfig.ControlType == InputActionType.Joystick)
				{
					// get combined action strength to produce axis strength
					_actionStates[actionName].Strength = Input.GetAxis(actionName+"-", actionName+"+");
				}
				else if (actionConfig.ControlType == InputActionType.Trigger)
				{
					_actionStates[actionName].Strength = Input.GetActionStrength(actionName, true);
				}
			}
		}
	}

	public void UpdateMouseState()
	{
		var mousePosition = GetViewport().GetMousePosition();

		_mouseState.Position = mousePosition;

		// TODO: figure out why this takes 14ms??
		// var mouseVelocity = Input.GetLastMouseVelocity();
        //
		// _mouseState.VelocityX = mouseVelocity.X;
		// _mouseState.VelocityY = mouseVelocity.Y;

		_mouseState.LeftButtonPressed = Input.IsMouseButtonPressed(MouseButton.Left);
		_mouseState.RightButtonPressed = Input.IsMouseButtonPressed(MouseButton.Right);
		_mouseState.MiddleButtonPressed = Input.IsMouseButtonPressed(MouseButton.Middle);

		_mouseState.WheelUp = Input.IsMouseButtonPressed(MouseButton.WheelUp);
		_mouseState.WheelDown = Input.IsMouseButtonPressed(MouseButton.WheelDown);
		_mouseState.WheelLeft = Input.IsMouseButtonPressed(MouseButton.WheelLeft);
		_mouseState.WheelRight = Input.IsMouseButtonPressed(MouseButton.WheelRight);
	}

	public void UpdateJoypadState(bool updateUnavailable = false)
	{
		var joypadIds = Input.GetConnectedJoypads();

		// set any existing joypads as unavailable
		foreach (var joyState in _joypadStates)
		{
			if (!joypadIds.Contains(joyState.Value.CurrentDeviceId))
			{
				joyState.Value.Available = false;

				_joypadGuidMappings.Remove(joyState.Value.CurrentDeviceId);

				LoggerManager.LogDebug($"Joypad {joyState.Value.Name} no longer available");

				this.Emit<InputStateJoypadUnavailable>(e => e.JoypadGuid = joyState.Key);

				_triggerConfiguration = true;
			}
		}

		// set joypads states
		foreach (int joyId in joypadIds)
		{
			var joyName = Input.GetJoyName(joyId);
			var joyGuid = Input.GetJoyGuid(joyId);

			// set joyID's GUID
			_joypadGuidMappings[joyId] = Input.GetJoyGuid(joyId);

			if (!_joypadStates.TryGetValue(joyGuid, out var joyState))
			{
				joyState = new JoypadState();
				joyState.Name = joyName;
				joyState.Guid = joyGuid;
				joyState.CurrentDeviceId = joyId;
				_joypadStates[joyGuid] = joyState;

				LoggerManager.LogDebug($"Adding joypad {joyName} as ID {joyId}", "", joyGuid, joyName);

				this.Emit<InputStateJoypadAvailable>(e => e.JoypadGuid = joyGuid);

				_triggerConfiguration = true;
			}

			joyState.Available = true;

			// set axes values
			foreach (JoyAxis i in new JoyAxis[] {JoyAxis.LeftX, JoyAxis.LeftY, JoyAxis.RightX, JoyAxis.RightY, JoyAxis.TriggerLeft, JoyAxis.TriggerRight})
			{
				joyState.SetAxisState(i, Input.GetJoyAxis(joyId, i));
			}

			// set joypad buttons
			foreach (JoyButton i in new JoyButton[] {
					JoyButton.A, 
					JoyButton.B, 
					JoyButton.X, 
					JoyButton.Y, 
					JoyButton.DpadUp, 
					JoyButton.DpadDown, 
					JoyButton.DpadLeft, 
					JoyButton.DpadRight, 
					JoyButton.LeftShoulder, 
					JoyButton.RightShoulder, 
					JoyButton.LeftStick, 
					JoyButton.RightStick, 
					JoyButton.Start, 
					JoyButton.Back, 
					JoyButton.Guide, 
					JoyButton.Touchpad, 
					JoyButton.Paddle1, 
					JoyButton.Paddle2, 
					JoyButton.Paddle3, 
					JoyButton.Paddle4, 
					JoyButton.Misc1, 
				})
			{
				joyState.SetButtonState(i, Input.IsJoyButtonPressed(joyId, i));
			}
		}
	}

	/***********************************
	*  Input action state management  *
	***********************************/
	
	public void _On_Input(InputEvent @e = null)
	{
		foreach (var action in InputMap.GetActions())
		{
			if (Input.IsActionJustPressed(action))
			{
				LoggerManager.LogDebug("Action just pressed", "", "action", action);
			}
			else if (Input.IsActionPressed(action))
			{
				LoggerManager.LogDebug("Action pressed", "", "action", action);
			}
			else if (Input.IsActionJustReleased(action))
			{
				LoggerManager.LogDebug("Action just released", "", "action", action);
			}
		}
	}

	/********************
	*  Event handlers  *
	********************/
	
	public void _On_InputEvent(InputEvent @e)
	{
		if (@e is InputEventMouseMotion)
		{
			UpdateMouseState();
			return;
		}
		if (@e is InputEventJoypadMotion)
		{
			// if we don't care to emit inputstate events for joypad motion,
			// then simply update the internal state and continue execution
			if (!_emitJoypadStateEvents)
			{
				UpdateJoypadState();
				return;
			}
		}

		LoggerManager.LogDebug("Input event", "", "event", @e);

		_processingInputEvent = true;

		_inputState.Transition(PROCESSING_STATE, true);
	}
}

public class ActionInputState 
{
	private bool _pressed;
	public bool Pressed
	{
		get { return _pressed; }
		set { _pressed = value; }
	}

	private bool _justPressed;
	public bool JustPressed
	{
		get { return _justPressed; }
		set { _justPressed = value; }
	}

	private bool _justReleased;
	public bool JustReleased
	{
		get { return _justReleased; }
		set { _justReleased = value; }
	}

	private InputActionConfig _actionConfig;
	public InputActionConfig Config
	{
		get { return _actionConfig; }
		set { _actionConfig = value; }
	}

	private float _strength;
	public float Strength
	{
		get { return _strength; }
		set { _strength = value; }
	}
}

public class MouseState 
{
	private Vector2 _position;
	public Vector2 Position
	{
		get { return _position; }
		set { _position = value; }
	}

	private Vector2 _velocity;
	public Vector2 Velocity
	{
		get { return _velocity; }
		set { _velocity = value; }
	}

	private bool _leftButtonPressed;
	public bool LeftButtonPressed
	{
		get { return _leftButtonPressed; }
		set { _leftButtonPressed = value; }
	}

	private bool _rightButtonPressed;
	public bool RightButtonPressed
	{
		get { return _rightButtonPressed; }
		set { _rightButtonPressed = value; }
	}

	private bool _middleButtonPressed;
	public bool MiddleButtonPressed
	{
		get { return _middleButtonPressed; }
		set { _middleButtonPressed = value; }
	}

	private bool _wheelUp;
	public bool WheelUp
	{
		get { return _wheelUp; }
		set { _wheelUp = value; }
	}

	private bool _wheelDown;
	public bool WheelDown
	{
		get { return _wheelDown; }
		set { _wheelDown = value; }
	}

	private bool _wheelLeft;
	public bool WheelLeft
	{
		get { return _wheelLeft; }
		set { _wheelLeft = value; }
	}

	private bool _wheelRight;
	public bool WheelRight
	{
		get { return _wheelRight; }
		set { _wheelRight = value; }
	}
}

public class JoypadState
{
	private string _name;
	public string Name
	{
		get { return _name; }
		set { _name = value; }
	}

	private string _guid;
	public string Guid
	{
		get { return _guid; }
		set { _guid = value; }
	}

	private int _currentDeviceId;
	public int CurrentDeviceId
	{
		get { return _currentDeviceId; }
		set { _currentDeviceId = value; }
	}

	private Dictionary<JoyAxis, float> _axes = new();
	public Dictionary<JoyAxis, float> Axes
	{
		get { return _axes; }
		set { _axes = value; }
	}

	private Dictionary<JoyButton, bool> _buttons = new();
	public Dictionary<JoyButton, bool> Buttons
	{
		get { return _buttons; }
		set { _buttons = value; }
	}

	private bool _available;
	public bool Available
	{
		get { return _available; }
		set { _available = value; }
	}

	public void SetAxisState(JoyAxis axisId, float axisValue)
	{
		_axes[axisId] = axisValue;
	}

	public void SetButtonState(JoyButton buttonId, bool pressed)
	{
		_buttons[buttonId] = pressed;
	}
}
