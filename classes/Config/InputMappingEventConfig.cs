/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : InputMappingConfig
 * @created     : Thursday Nov 23, 2023 12:53:45 CST
 */

namespace GodotEGP.Config;

using System;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class InputMappingEventConfig : VConfig
{
	internal readonly VValue<List<InputMappingEvent>> _mappingEvents;

	public List<InputMappingEvent> Events
	{
		get { return _mappingEvents.Value; }
		set { _mappingEvents.Value = value; }
	}

	public InputMappingEventConfig()
	{
		_mappingEvents = AddValidatedValue<List<InputMappingEvent>>(this)
		    .Default(new List<InputMappingEvent>())
		    .ChangeEventsEnabled();

		// var e1 = new InputMappingEvent();
		// e1.Keycode = Key.A;
        //
		// var e2 = new InputMappingEvent();
		// e2.JoypadButton = JoyButton.A;
        //
		// Events.Add(e1);
		// Events.Add(e2);
	}
}

public partial class InputMappingEvent
{
	private string _deviceGuid = "";
	public string DeviceGuid
	{
		get { return _deviceGuid; }
		set { _deviceGuid = value; }
	}

	private JoyButton _joypadButton = JoyButton.Invalid;
	public JoyButton JoypadButton
	{
		get { return _joypadButton; }
		set { _joypadButton = value; }
	}

	private JoyAxis _joyAxis = JoyAxis.Invalid;
	public JoyAxis JoypadAxis
	{
		get { return _joyAxis; }
		set { _joyAxis = value; }
	}

	private int _joyAxisDirection = -1;
	public int AxisDirection
	{
		get { return _joyAxisDirection; }
		set { _joyAxisDirection = value; }
	}

	private Key _keycode = Key.None;
	public Key Keycode
	{
		get { return _keycode; }
		set { _keycode = value; }
	}

	internal bool _echo = false;
	public bool Echo
	{
		get { return _echo = false; }
		set { _echo = value; }
	}

	private bool _altPressed = false;
	public bool AltPressed
	{
		get { return _altPressed; }
		set { _altPressed = value; }
	}

	private bool _ctrlPressed = false;
	public bool CtrlPressed
	{
		get { return _ctrlPressed; }
		set { _ctrlPressed = value; }
	}

	private bool _metaPressed = false;
	public bool MetaPressed
	{
		get { return _metaPressed; }
		set { _metaPressed = value; }
	}

	private bool _shiftPressed = false;
	public bool ShiftPressed
	{
		get { return _shiftPressed; }
		set { _shiftPressed = value; }
	}

	private MouseButton _mouseButton = MouseButton.None;
	public MouseButton MouseButton
	{
		get { return _mouseButton; }
		set { _mouseButton = value; }
	}

	private bool _doubleClick = false;
	public bool DoubleClick
	{
		get { return _doubleClick = false; }
		set { _doubleClick = value; }
	}

	internal bool IsMouseMapping
	{
		get { return (_mouseButton != MouseButton.None); }
	}
	internal bool IsKeyboardMapping
	{
		get { return (_keycode != Key.None); }
	}
	internal bool IsJoypadMapping
	{
		get { return (_joypadButton != JoyButton.Invalid || _joyAxis != JoyAxis.Invalid); }
	}

	public InputMappingEvent()
	{

	}

	public InputEvent ToInputEvent()
	{
		// mouse button event
		if (_mouseButton != MouseButton.None)
		{
			var e = new InputEventMouseButton();

			e.ButtonIndex = _mouseButton;
			e.DoubleClick = _doubleClick;
			e.Pressed = true;

			// include keyboard modifier keys
			e.MetaPressed = _metaPressed;
			e.ShiftPressed = _shiftPressed;
			e.CtrlPressed = _ctrlPressed;
			e.AltPressed = _altPressed;

			return e;
		}

		// key event
		if (_keycode != Key.None)
		{
			var e = new InputEventKey();

			e.Keycode = _keycode;
			e.Pressed = true;

			e.MetaPressed = _metaPressed;
			e.ShiftPressed = _shiftPressed;
			e.CtrlPressed = _ctrlPressed;
			e.AltPressed = _altPressed;
			e.Echo = _echo;

			return e;
		}

		// joypad button event
		if (_joypadButton != JoyButton.Invalid)
		{
			var e = new InputEventJoypadButton();

			e.ButtonIndex = _joypadButton;
			e.Pressed = true;

			return e;
		}

		// joypad button event
		if (_joyAxis != JoyAxis.Invalid)
		{
			var e = new InputEventJoypadMotion();

			e.Axis = _joyAxis;

			// TODO: incorporate axis direction into the event somehow

			return e;
		}

		return null;
	}
}
