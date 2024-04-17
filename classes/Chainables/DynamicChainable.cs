/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : DynamicChainable
 * @created     : Friday Apr 05, 2024 15:50:49 CST
 */

namespace GodotEGP.Chainables;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Chainables.Exceptions;
using GodotEGP.Chainables.Extensions;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class DynamicChainable : ChainableAssign
{
	public IChainable Target { get; set; }

	public async Task<object> Run(object? input = null)
	{
		if (Target != null)
			return await Target.Config(this.Config).Run(input);
		else
			return await base.Run(input);
	}
	public async Task<object[]> Batch(object[]? batchInput = null)
	{
		if (Target != null)
			return await Target.Config(this.Config).Batch(batchInput);
		else
			return await base.Batch(batchInput);
	}
	public async IAsyncEnumerable<object> Stream(object? input = null)
	{
		if (Target != null)
		{
			await foreach (var output in Target.Config(this.Config).Stream(input))
			{
				yield return output;
			}
		}
		else
		{
			await foreach (var output in base.Stream(input))
			{
				yield return output;
			}
		}
	}
}
