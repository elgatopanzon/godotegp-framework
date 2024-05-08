/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ECSConfig
 * @created     : Thursday May 02, 2024 11:52:42 CST
 */

namespace GodotEGP.Config;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class ECSConfig : VConfig
{
	partial void InitConfigParams();

	internal readonly VValue<uint> _entityIdRangeMin;

	public uint EntityIdRangeMin
	{
		get { return (EntityIdRangeCheckEnabled) ? _entityIdRangeMin.Value : 0; }
		set { _entityIdRangeMin.Value = value; }
	}

	internal readonly VValue<uint> _entityIdRangeMax;

	public uint EntityIdRangeMax
	{
		get { return (EntityIdRangeCheckEnabled) ? _entityIdRangeMax.Value : 0; }
		set { _entityIdRangeMax.Value = value; }
	}

	internal readonly VValue<bool> _entityIdRangeCheckEnabled;

	public bool EntityIdRangeCheckEnabled
	{
		get { return _entityIdRangeCheckEnabled.Value; }
		set { _entityIdRangeCheckEnabled.Value = value; }
	}

	internal readonly VValue<bool> _keepQueryResultsUpdated;

	public bool KeepQueryResultsUpdated
	{
		get { return _keepQueryResultsUpdated.Value; }
		set { _keepQueryResultsUpdated.Value = value; }
	}
	
	public ECSConfig()
	{
		_entityIdRangeMin = AddValidatedValue<uint>(this)
		    .Default(5000)
		    .ChangeEventsEnabled();

		_entityIdRangeMax = AddValidatedValue<uint>(this)
		    .Default(0)
		    .ChangeEventsEnabled();

		_entityIdRangeCheckEnabled = AddValidatedValue<bool>(this)
		    .Default(true)
		    .ChangeEventsEnabled();

		_keepQueryResultsUpdated = AddValidatedValue<bool>(this)
		    .Default(true)
		    .ChangeEventsEnabled();
	}
}
