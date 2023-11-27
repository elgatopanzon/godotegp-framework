/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : Definition
 * @created     : Saturday Nov 11, 2023 14:14:06 CST
 */

namespace GodotEGP.Resource;

using System;
using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class Definition : VObject
{
	internal readonly VValue<string> _path;

	public string Path
	{
		get { return _path.Value; }
		set { _path.Value = value; }
	}

	internal readonly VValue<string> _resourceClass;

	public string Class
	{
		get { return _resourceClass.Value; }
		set { 
			_resourceClass.Value = value;

			_resourceType = GetResourceType(value);
		}
	}

	private Type _resourceType;
	public Type ClassType
	{
		get { return _resourceType; }
		set { _resourceType = value; }
	}

	// store config options to be applied to the loaded resources
	private readonly VValue<Dictionary<string, string>> _configDictionary;

	public Dictionary<string, string> Config
	{
		get { return _configDictionary.Value; }
		set { _configDictionary.Value = value; }
	}

	public Definition()
	{
		_path = AddValidatedValue<string>(this)
			.NotNull();

		_resourceClass = AddValidatedValue<string>(this)
			.NotNull();

        _configDictionary = AddValidatedValue<Dictionary<string, string>>(this)
            .Default(new Dictionary<string, string>() {
            	})
            ;
	}

	public Type GetResourceType(string typeString)
	{
		foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
     	{
         	Type type = asm.GetType(typeString);

         	if (type != null)
         	{
         		return type;

         	}
        }

        throw new TypeLoadException($"Type {typeString} isn't a valid Type!");
	}

	public bool IsResourcePath()
	{
		return (_path.Value != null && _path.Value.StartsWith("res://"));
	}
}

