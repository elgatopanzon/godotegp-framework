/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableBranch
 * @created     : Friday Apr 05, 2024 23:35:08 CST
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
using System.Threading.Tasks;

public partial class ChainableBranch : ChainablePassthrough
{
	public List<(Func<object, bool> Condition, IChainable Chain)> Branches { get; set; } = new();

	/************************
	*  Object pool methods  *
	************************/

	public override void Reset()
	{
		Branches = null;
		base.Reset();
	}
	
	public override void InitChainable(object? input = null)
	{
		Branches = new();
		base.InitChainable(input);
	}

	public void InitChainable(List<(Func<object, bool> Condition, IChainable Chain)> branches = null, IChainable defaultBranch = null)
	{
		// set the dynamic target to the default, or to a new passthrough
		if (defaultBranch == null)
			Target = this.CreateInstance<ChainablePassthrough>();
		else
			Target = defaultBranch;

		Branches = branches;

		base.InitChainable();
	}

	public ChainableBranch(List<(Func<object, bool> Condition, IChainable Chain)> branches = null, IChainable defaultBranch = null)
	{
		InitChainable(branches, defaultBranch);
	}

	public ChainableBranch Branch(IChainable conditionChain, IChainable chain)
	{
		var lambda = (object x) => (bool) conditionChain.Run(x).Result;
		Branches.Add((lambda, chain));

		return this;
	}

	public ChainableBranch Branch(Func<object, bool> condition, IChainable chain)
	{
		Branches.Add((condition, chain));

		return this;
	}

	public ChainableBranch Branch(Func<object, bool> condition, Func<object, object> lambda)
	{
		Branches.Add((condition, this.CreateInstance<ChainableLambda>(lambda)));

		return this;
	}

	public ChainableBranch Branch<T1, T2>(Func<object, bool> condition, Func<T1, T2> lambda)
	{
		Branches.Add((condition, this.CreateInstance<ChainableLambda<T1, T2>>(lambda)));

		return this;
	}

	public ChainableBranch Default(IChainable chain)
	{
		Target = chain;

		return this;
	}

	public ChainableBranch Default(Func<object, object> lambda)
	{
		Target = this.CreateInstance<ChainableLambda>(lambda);

		return this;
	}

	public ChainableBranch Default<T1, T2>(Func<T1, T2> lambda)
	{
		Target = this.CreateInstance<ChainableLambda<T1, T2>>(lambda);

		return this;
	}

	public async Task<object> Run(object? input = null)
	{
		Input = input;
		SetActiveBranch(Input);
		return await base.Run(Input);
	}
	public async Task<object[]> Batch(object[]? batchInput = null)
	{
		SetActiveBranch(batchInput);
		return await base.Batch(batchInput);
	}
	public async IAsyncEnumerable<object> Stream(object? input = null)
	{
		Input = input;
		SetActiveBranch(Input);
		await foreach (var output in base.Stream(Input))
		{
			yield return output;
		}
	}

	public virtual bool SetActiveBranch(object input)
	{
		LoggerManager.LogDebug("Evaluating branching conditions", "", "branches", Branches.Count);

		foreach (var branch in Branches)
		{
			bool res = branch.Condition(input);

			if (res)
			{
				LoggerManager.LogDebug("Branch condition matched");

				Target = branch.Chain;
				return true;
			}
		}

		LoggerManager.LogDebug("No branch condition was true");
		return false;
	}
}

