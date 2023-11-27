/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : InputEventConfig
 * @created     : Thursday Nov 23, 2023 13:47:11 CST
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

public partial class InputMappingConfig : VConfig
{
	internal readonly VValue<Dictionary<string, InputMappingEventConfig>> _mappingConfig;

	public Dictionary<string, InputMappingEventConfig> Mappings
	{
		get { return _mappingConfig.Value; }
		set { _mappingConfig.Value = value; }
	}

	public InputMappingConfig()
	{
		_mappingConfig = AddValidatedValue<Dictionary<string, InputMappingEventConfig>>(this)
		    .Default(new Dictionary<string, InputMappingEventConfig>())
		    .ChangeEventsEnabled();

		// var da = new InputMappingEventConfig();
        //
		// var e1 = new InputMappingEvent();
		// e1.Keycode = Key.A;
        //
		// da.Events.Add(e1);
        //
		// Mappings.Add("DefaultAction", da);
	}
}

