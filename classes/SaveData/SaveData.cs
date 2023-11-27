/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SaveData
 * @created     : Thursday Nov 09, 2023 17:03:15 CST
 */

namespace GodotEGP.SaveData;

using System;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public enum SaveDataType
{
	Manual,
	System,
	Autosave,
	Suspend,
	Backup
}

public partial class Data : VObject
{
	// keep track of the save structure and use it for future migrations
	internal readonly VValue<int> _saveVersion;

	public int SaveVersion
	{
		get { return _saveVersion.Value; }
		set { _saveVersion.Value = value; }
	}

	// save type easily indicated for processing/UI uses
	internal readonly VValue<SaveDataType> _saveType;

	public SaveDataType SaveType
	{
		get { return _saveType.Value; }
		set { _saveType.Value = value; }
	}

	// timestamps
	internal readonly VValue<DateTime> _dateCreated;

	public DateTime DateCreated
	{
		get { return _dateCreated.Value; }
		set { _dateCreated.Value = value; }
	}

	internal readonly VValue<DateTime> _dateSaved;

	public DateTime DateSaved
	{
		get { return _dateSaved.Value; }
		set { _dateSaved.Value = value; }
	}

	internal readonly VValue<DateTime> _dateLoaded;

	public DateTime DateLoaded
	{
		get { return _dateLoaded.Value; }
		set { _dateLoaded.Value = value; }
	}

	internal readonly VValue<DateTime> _dateAutosaved;

	internal DateTime DateAutosaved
	{
		get { return _dateAutosaved.Value; }
		set { _dateAutosaved.Value = value; }
	}

	// friendly name for the save (usually the same as the filename)
	internal readonly VValue<string> _name;

	public string Name
	{
		get { return _name.Value; }
		set { _name.Value = value; }
	}

	// indicates if save belongs to another save name
	internal readonly VValue<string> _parentName;

	public string ParentName
	{
		get { return _parentName.Value; }
		set { _parentName.Value = value; }
	}

	// indicate if the save is currently active and loaded
	internal readonly VValue<bool> _loaded;

	internal bool Loaded
	{
		get { return _loaded.Value; }
		set { 
			_loaded.Value = value;

			if (value == true)
			{
				UpdateDateLoaded();
			}
		}
	}

	public Data()
	{
        _saveVersion = AddValidatedValue<int>(this)
        	.Default(1)
        	.ChangeEventsEnabled();

        _saveType = AddValidatedValue<SaveDataType>(this)
        	.Default(SaveDataType.Manual)
        	.ChangeEventsEnabled();

        _dateCreated = AddValidatedValue<DateTime>(this)
        	.Default(DateTime.Now)
        	.ChangeEventsEnabled();

        _dateSaved = AddValidatedValue<DateTime>(this)
        	.Default(DateTime.Now)
        	.ChangeEventsEnabled();

        _dateLoaded = AddValidatedValue<DateTime>(this)
        	.Default(DateTime.Now)
        	.ChangeEventsEnabled();

		// set default as last saved time
        _dateAutosaved = AddValidatedValue<DateTime>(this)
        	.Default(_dateSaved.Value)
        	.ChangeEventsEnabled();

        _name = AddValidatedValue<string>(this)
        	.ChangeEventsEnabled();

		_parentName = AddValidatedValue<string>(this)
	    	.Default(null)
	    	.ChangeEventsEnabled();

		_loaded = AddValidatedValue<bool>(this)
	    	.Default(false)
	    	.ChangeEventsEnabled();

	}

	public void UpdateDateSaved()
	{
		_dateSaved.Value = DateTime.Now;

		LoggerManager.LogDebug("Updating saved date", "", "date", _dateSaved.Value);
	}
	public void UpdateDateLoaded()
	{
		_dateLoaded.Value = DateTime.Now;

		LoggerManager.LogDebug("Updating loaded date", "", "date", _dateLoaded.Value);
	}

	public bool AutosaveSupported()
	{
		// ignore backup and autosave saves from being backed up
		return (SaveType != SaveDataType.Autosave && SaveType != SaveDataType.Backup);
	}
}

// single save data with VValues
public partial class SystemData : Data
{
	public SystemData() : base()
	{
		_saveType.Value = SaveDataType.System;
	}
}


// complex container with encompassing SaveData objects
public partial class GameSaveFile : Data
{
	public GameSaveFile() : base()
	{
	}
}
