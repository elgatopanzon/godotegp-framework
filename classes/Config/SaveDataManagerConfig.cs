/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SaveDataManagerConfig
 * @created     : Friday Nov 10, 2023 14:45:35 CST
 */

namespace GodotEGP.Config;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class SaveDataManagerConfig : VObject
{
	// configure autosave system enabled or disabled
	internal readonly VValue<bool> _timedAutosaveEnabled;

	public bool TimedAutosaveEnabled
	{
		get { return _timedAutosaveEnabled.Value; }
		set { _timedAutosaveEnabled.Value = value; }
	}

	// configure maximum number of autosaves to keep per save
	internal readonly VValue<int> _autosaveMax;

	public int AutosaveMax
	{
		get { return _autosaveMax.Value; }
		set { _autosaveMax.Value = value; }
	}

	// configure the default time interval between autosaves
	internal readonly VValue<int> _autosaveTimeDefaultSec;

	public int AutosaveTimeDefaultSec
	{
		get { return _autosaveTimeDefaultSec.Value; }
		set { _autosaveTimeDefaultSec.Value = value; }
	}


	public SaveDataManagerConfig(VObject parent = null) : base(parent)
	{
		_timedAutosaveEnabled = AddValidatedValue<bool>(this)
		    .Default(true)
		    .ChangeEventsEnabled();
		_autosaveMax = AddValidatedValue<int>(this)
		    .Default(5)
		    .ChangeEventsEnabled();
		_autosaveTimeDefaultSec = AddValidatedValue<int>(this)
	    	.Default(15 * 86400)
	    	.ChangeEventsEnabled();
	}
}

