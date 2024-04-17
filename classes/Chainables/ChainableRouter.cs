/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableRouter
 * @created     : Saturday Apr 06, 2024 00:29:35 CST
 */

namespace GodotEGP.Chainables;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System;
using System.Collections.Generic;

public partial class ChainableRouter : ChainableBranch
{
	public Dictionary<string, IChainable> Routes { get; set; } = new();
	public string RouteKey { get; set; } = "key";

	public ChainableRouter(Dictionary<string, IChainable> routes = null)
	{
		if (routes != null)
			Routes = routes;
	}

	public ChainableRouter Route(string name, IChainable chain)
	{
		Routes[name] = chain;

		return this;
	}

	public override bool SetActiveBranch(object input)
	{
		BuildBranchConditionsFromRoutes();

		var branchRes = base.SetActiveBranch(input);

		if (branchRes == false)
		{
			throw new ArgumentException($"Route key '{RouteKey}' not present or no matching routes!");
		}

		if (input is Dictionary<string, object> d)
		{
			if (!d.TryGetValue("input", out var i))
			{
				throw new ArgumentException("Missing 'input' key in dictionary!");
			}
			Input = i;
		}
		else
		{
			throw new ArgumentException($"Input must be a dictionary with route key '{RouteKey}' and 'input' fields!");
		}

		return branchRes;
	}

	public void BuildBranchConditionsFromRoutes()
	{
		Branches = new();

		foreach (var route in Routes)
		{
			this.Branch((x) => (x as Dictionary<string, object>)[RouteKey] == route.Key, route.Value);
		}
	}
}
