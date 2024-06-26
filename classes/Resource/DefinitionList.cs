/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : DefinitionList
 * @created     : Saturday Nov 11, 2023 14:14:16 CST
 */

namespace GodotEGP.Resource;

using System.Collections.Generic;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

public partial class DefinitionList : VObject
{
	private readonly VValue<Dictionary<string, ResourceDefinition>> _images;

	public Dictionary<string, ResourceDefinition> Images
	{
		get { return _images.Value; }
		set { _images.Value = value; }
	}

	public DefinitionList()
	{
        _images = AddValidatedValue<Dictionary<string, ResourceDefinition>>(this)
            .Default(new Dictionary<string, ResourceDefinition>() {
            	})
            ;
            _images.MergeCollections = true;
	}
}
