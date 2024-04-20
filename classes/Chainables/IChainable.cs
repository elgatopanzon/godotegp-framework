/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : IChainable
 * @created     : Wednesday Mar 27, 2024 13:28:05 CST
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

public interface IChainable
{
	public string Name { get; }

	// chain input properties
	public object Input { get; set; }
	public object Output { get; set; }

	// input coerce methods
	public bool TryInputAs<T>(out T c);
	public bool TryOutputAs<T>(out T c);
	
	// chain stacking properties
	public List<IChainable> Stack { get; set; }
	public object StackInput { get; set; }
	public object StackOutput { get; set; }
	public bool IsStack { get; }

	// chain input methods
	public Task<object> Run(object? input = null);
	public Task<object[]> Batch(object[]? batchInput = null);
	public IAsyncEnumerable<object> Stream(object? input = null);

	public Task<T2> Run<T1, T2>(object? input = null);
	public Task<List<T2>> Batch<T1, T2>(List<object> batchInput = null)
		where T1 : notnull, new()
		where T2 : notnull, new();
	public Task<List<T2>> Batch<TInput, T1, T2>(List<TInput> batchInput = null)
		where T1 : notnull, new()
		where T2 : notnull, new();
	public IAsyncEnumerable<T2> Stream<T1, T2>(object? input = null);

	// chain process methods
	public Task<object> _Process();

	// chain stream methods
	public IAsyncEnumerable<object> _ProcessStream();
	public IAsyncEnumerable<object> StreamTransform(IAsyncEnumerable<object> input = null);
	public bool HasOwnProcessStreamMethod();
	public bool HasOwnStreamTransformMethod();

	// error handling
	public Exception Error { get; set; }
	public bool Success { get; }

	// schema
	public ChainableSchema Schema { get; }
	public ChainableSchemaDefinition InputSchema { get; }
	public ChainableSchemaDefinition OutputSchema { get; }

	// config
	public ChainableConfig Config { get; set; }

	public bool RunWithConfig { get; set; }
	public IChainable Clone();
	public void ResetChainable();
}

public interface IChainableInput {};
public interface IChainableOutput {};
public interface IChainableInput<T> : IChainableInput {};
public interface IChainableOutput<T> : IChainableOutput {};
