/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : SceneTransitionManager
 * @created     : Sunday Nov 12, 2023 21:48:18 CST
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

public partial class SceneTransitionManagerConfig : VObject
{
	internal readonly VValue<Dictionary<string, List<SceneTransitionChainItem>>> _transitionChains;

	public  Dictionary<string, List<SceneTransitionChainItem>> TransitionChains
	{
		get { return _transitionChains.Value; }
		set { _transitionChains.Value = value; }
	}

	public SceneTransitionManagerConfig(VObject parent = null) : base(parent)
	{
		_transitionChains = AddValidatedValue<Dictionary<string, List<SceneTransitionChainItem>>>(this)
	    	.Default(new Dictionary<string, List<SceneTransitionChainItem>>())
	    	.ChangeEventsEnabled();
	}
}

