/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainablePick
 * @created     : Friday Apr 05, 2024 22:27:46 CST
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
using System.Threading.Tasks;
using System.Collections.Generic;

public partial class ChainablePick : ChainablePassthrough
{
	public string[] Keys { get; set; }

	public ChainablePick(string[] keys)
	{
		Keys = keys;
	}
	public ChainablePick(string key)
	{
		Keys = new string[] { key };
	}

	public async override Task<object> _Process()
	{
		return GetPickedValues();
	}

	public async override IAsyncEnumerable<object> _ProcessStream()
	{
		yield return GetPickedValues();
	}

	public Dictionary<string, object> GetPickedValues()
	{
		LoggerManager.LogDebug("Picking keys from input dictionary", "", "keys", Keys);

		Dictionary<string, object> picked = new();

		if (Input is Dictionary<string, object> d)
		{
			foreach (var key in Keys)
			{
				picked[key] = d[key];
			}
		}

		return picked;
	}
}

