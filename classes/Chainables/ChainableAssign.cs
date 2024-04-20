/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableAssign
 * @created     : Tuesday Apr 02, 2024 23:10:37 CST
 */

namespace GodotEGP.Chainables;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using GodotEGP.Chainables.Extensions;

using System.Collections.Generic;
using System.Threading.Tasks;

public partial class ChainableAssign : Chainable, IChainable, IChainableAssign
{
	public Dictionary<string, object> AssignedOutputs { get; set; } = new();


	/************************
	*  Object pool methods  *
	************************/

	public override void Reset()
	{
		AssignedOutputs = null;
		base.Reset();
	}
	
	public override void InitChainable(object? input = null)
	{
		AssignedOutputs = new();
		base.InitChainable(input);
	}



	public async Task<Dictionary<string, object>> Run(Dictionary<string, object> input = null)
	{
		return (Dictionary<string, object>) await base.Run(input);
	}

	public async Task<Dictionary<string, object>[]> Batch(Dictionary<string, object>[] batchInput = null)
	{
		return (Dictionary<string, object>[]) await base.Batch(batchInput);
	}
	public async IAsyncEnumerable<Dictionary<string, object>> Stream(Dictionary<string, object> input = null)
	{
		await foreach (var output in base.Stream(input))
		{
			yield return (Dictionary<string, object>) output;
		}
	}

	// use the post execution hook override to execute and assign the values
	public async override void _HookPostExecution()
	{
		// if the Output isn't a dictionary we have to create it, with the
		// default key being "input"
		Dictionary<string, object> assignedValues = new();

		foreach (var assignable in AssignedOutputs)
		{
			LoggerManager.LogDebug($"Assigning {assignable.Value.GetType()} value to output", "", assignable.Key, assignable.Value.ToString());

			assignedValues[assignable.Key] = await TryGetChainableResult(assignable.Value, Output);
		}

		// if there's any assigned values then convert the output value to a
		// dictionary and merge with the assigned values
		if (assignedValues.Count > 0)
		{
			// assign the assigned values to the Output if it's a dictionary already
			if (Output is Dictionary<string, object> d)
			{
				LoggerManager.LogDebug("Pre-merging assigned values", "", "output", Output);

				var clone = d.ShallowClone();
				clone.Merge(assignedValues);

				LoggerManager.LogDebug("Clone assigned values", "", "output", clone);

				Output = clone;
			}

			// if it's not a dictionary, make it one so we output and return a
			// proper dictionary
			else
			{
				assignedValues.Merge(new() {{ "input", Output }});
				Output = assignedValues;
			}

			LoggerManager.LogDebug("Final output", "", "Output", Output);
		}

		base._HookPostExecution();
	}

	public ChainableValueGetter ValueGetter(string key)
	{
		var c = this.CreateInstance<ChainableValueGetter>();

		c = c.Param("Key", key);

		return c;
	}
}

// assigns the output of the chainable to the value of the key given in the
// input dictionary
public partial class ChainableValueGetter : ChainableAssign
{
	// the dictionary key
	public string Key { get; set; }

	public override void Reset()
	{
		Key = null;
		base.Reset();
	}

	public async override Task<object> _Process()
	{
		return GetValue();
	}

	public async override IAsyncEnumerable<object> _ProcessStream()
	{
		yield return GetValue();
	}

	public object GetValue()
	{
		// get the input key from the input dictionary
		var value = (Input as Dictionary<string, object>)[Key];

		LoggerManager.LogDebug("Get value from input dictionary", "", Key, value);

		return value;
	}
}
