/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : InputManagerConfig
 * @created     : Thursday Nov 23, 2023 11:39:50 CST
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

public partial class InputManagerConfig : VConfig
{
	private VValue<Dictionary<string, InputActionConfig>> _inputActions;
	public Dictionary<string, InputActionConfig> Actions
	{
		get { return _inputActions.Value; }
		set { _inputActions.Value = value; }
	}

	public InputManagerConfig()
	{
        _inputActions = AddValidatedValue<Dictionary<string, InputActionConfig>>(this)
            .Default(new() {
            	})
            ;
            _inputActions.MergeCollections = true;
	}
}

