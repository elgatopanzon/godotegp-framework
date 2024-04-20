/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableConfig
 * @created     : Monday Apr 01, 2024 13:02:52 CST
 */

namespace GodotEGP.Chainables;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.ObjectPool;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Collections.Generic;

public partial class ChainableConfig : IPoolableObject
{
	public List<string> Tags { get; set; } = new();
	public Dictionary<string, object> Metadata { get; set; } = new();
	public string RunName { get; set; }
	public int MaxConcurrency { get; set; }
	public Dictionary<string, ChainableConfigurableParam> ConfigurableParams { get; set; } = new();
	public Dictionary<string, object> Params { get; set; } = new();

	/************************
	*  Object pool methods  *
	************************/

	public virtual void Reset()
	{
		Tags = null;
		Metadata = null;
		RunName = null;
		MaxConcurrency = 0;
		ConfigurableParams = null;
		Params = null;
	}
	
	public virtual void Init(params object[] p)
	{
		Tags = new();
		Metadata = new();
		MaxConcurrency = 0;
		ConfigurableParams = new();
		Params = new();
	}

	public void Dispose()
	{
		Reset();
	}


	public ChainableConfig Clone()
	{
		var clone = (ChainableConfig) this.MemberwiseClone();

		// clone objects
		clone.Tags = this.Tags.ShallowClone();
		clone.Metadata = this.Metadata.ShallowClone();
		clone.ConfigurableParams = this.ConfigurableParams.ShallowClone();
		clone.Params = this.Params.ShallowClone();

		return clone;
	}
}
