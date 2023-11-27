/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : GlobalConfig
 * @created     : Saturday Nov 11, 2023 00:30:00 CST
 */

namespace GodotEGP.Config;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class GlobalConfig : VConfig
{
	// used for migrations
	internal readonly VValue<int> _configVersion;

	public int ConfigVersion
	{
		get { return _configVersion.Value; }
		set { _configVersion.Value = value; }
	}

	internal readonly VNative<InputMappingConfig> _mappingConfig;

	public InputMappingConfig InputMapping
	{
		get { return _mappingConfig.Value; }
		set { _mappingConfig.Value = value; }
	}

	public GlobalConfig()
	{
		_configVersion = AddValidatedValue<int>(this)
		    .Default(1)
		    .ChangeEventsEnabled();

		_mappingConfig = AddValidatedNative<InputMappingConfig>(this)
		    .Default(new InputMappingConfig())
		    .ChangeEventsEnabled();
	}
}

