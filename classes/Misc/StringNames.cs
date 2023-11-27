/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : StringNames
 * @created     : Sunday Nov 19, 2023 22:32:35 CST
 */

namespace GodotEGP.Misc;

using System;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class StringNames : LazySingleton<StringNames>
{
	private Dictionary<string, StringName> _stringNames = new Dictionary<string, StringName>();

	public StringName this[string stringName]
	{
    	get
    	{
    		return Get(stringName);
    	}
	}

	public static StringName Get(string stringName) 
	{
		if (!Instance._stringNames.TryGetValue(stringName, out StringName sn))
		{
			sn = new StringName(stringName);
			Set(stringName, sn);

			LoggerManager.LogDebug("Creating StringName instance", "", "stringName", stringName);
		}

		return sn;
	}

	public static void Set(string stringName, StringName sn)
	{
		if (!Instance._stringNames.TryAdd(stringName, sn))
		{
			throw new StringNameExistsException($"StringName instance already exists with name {stringName}!");
		}
	}

	public static void Remove(string stringName)
	{
		Instance._stringNames.Remove(stringName);
	}
}

public class StringNameExistsException : Exception
{
	public StringNameExistsException() {}
	public StringNameExistsException(string message) : base(message) {}
	public StringNameExistsException(string message, Exception inner) : base(message, inner) {}
}
