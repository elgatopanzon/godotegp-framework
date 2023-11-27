/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : InputActionConfig
 * @created     : Thursday Nov 23, 2023 11:33:13 CST
 */

namespace GodotEGP.Config;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public enum InputActionType
{
	Button = 0,
	Trigger = 1,
	Joystick = 2
}

public partial class InputActionConfig : VConfig
{
	internal readonly VValue<InputActionType> _controlType;

	public InputActionType ControlType
	{
		get { return _controlType.Value; }
		set { _controlType.Value = value; }
	}

	internal readonly VValue<double> _controlDeadzone;

	public double Deadzone
	{
		get { return _controlDeadzone.Value; }
		set { _controlDeadzone.Value = value; }
	}

	internal readonly VValue<int> _playerSlot;

	public int Player
	{
		get { return _playerSlot.Value; }
		set { _playerSlot.Value = value; }
	}

	public InputActionConfig()
	{
		_controlType = AddValidatedValue<InputActionType>(this)
		    .Default(InputActionType.Button)
		    .ChangeEventsEnabled();

		_controlDeadzone = AddValidatedValue<double>(this)
		    .Default(0.0)
		    .ChangeEventsEnabled();

		_playerSlot = AddValidatedValue<int>(this)
		    .Default(0)
		    .ChangeEventsEnabled();
	}
}

