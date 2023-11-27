/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ResourceDefinitionConfig
 * @created     : Saturday Nov 11, 2023 14:11:18 CST
 */

namespace GodotEGP.Config;

using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;
using GodotEGP.Resource;

public partial class ResourceDefinitionConfig : VConfig
{
	// internal readonly VNative<DefinitionList> _resourceDefinitions;
    //
	// public DefinitionList Resources
	// {
	// 	get { return _resourceDefinitions.Value; }
	// 	set { _resourceDefinitions.Value = value; }
	// }
	//
	private readonly VValue<Dictionary<string, Dictionary<string, Definition>>> _resourceDefinitions;

	public Dictionary<string, Dictionary<string, Definition>> Resources
	{
		get { return _resourceDefinitions.Value; }
		set { _resourceDefinitions.Value = value; }
	}

	public ResourceDefinitionConfig()
	{
		// _resourceDefinitions = AddValidatedNative<DefinitionList>(this)
		//     .Default(new DefinitionList())
		//     .ChangeEventsEnabled();

        _resourceDefinitions = AddValidatedValue<Dictionary<string, Dictionary<string, Definition>>>(this)
            .Default(new Dictionary<string,Dictionary<string, Definition>>() {
            	})
            ;
            _resourceDefinitions.MergeCollections = true;
	}
}

