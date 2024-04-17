/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableFallback
 * @created     : Saturday Apr 06, 2024 15:49:10 CST
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
using System.Linq;

public partial class ChainableFallback : ChainablePassthrough
{
	public string ExceptionKey { get; set; } = "Exceptions";
	public List<Type> FallbackExceptions { get; set; } = new() { typeof(Exception) };
	public List<IChainable> Fallbacks { get; set; } = new();
	public Stack<IChainable> FallbackStack { get; set; }
	public IChainable Chainable { get; set; }

	public ChainableFallback(string exceptionKey = null, List<Type> fallbackExceptions = null, List<IChainable> fallbacks = null, IChainable chainable = null)
	{
		if (exceptionKey != null)
			ExceptionKey = exceptionKey;
		if (fallbackExceptions != null)
			FallbackExceptions = fallbackExceptions;
		if (fallbacks != null)
			Fallbacks = fallbacks;
		if (chainable != null)
			Chainable = chainable;

	}

	public async Task<object> Run(object? input = null)
	{
		Target = Chainable;
		FallbackStack = null;
		Input = input;

		return await RunWithFallback(Input);
	}
	public async Task<object[]> Batch(object[]? batchInput = null)
	{
		Target = Chainable;
		FallbackStack = null;

		return await BatchWithFallback(batchInput);
	}
	public async IAsyncEnumerable<object> Stream(object? input = null)
	{
		Target = Chainable;
		FallbackStack = null;
		Input = input;

		await foreach (var output in StreamWithFallback(Input))
		{
			yield return output;
		}
	}

	public async Task<object> RunWithFallback(object? input = null)
	{
		try
		{
			return await base.Run(input);
		}
		catch (System.Exception e)
		{
			if (HandleThrownException(e) != null)
				throw;

			return await RunWithFallback(input);
		}
	}
	public async Task<object[]> BatchWithFallback(object[]? batchInput = null)
	{
		try
		{
			return await base.Batch(batchInput);
		}
		catch (System.Exception e)
		{
			if (HandleThrownException(e) != null)
				throw;

			return await BatchWithFallback(batchInput);
		}
	}
	public async IAsyncEnumerable<object> StreamWithFallback(object? input = null)
	{
		IAsyncEnumerator<object> enumerator=null!;
        try
        {
            enumerator = base.Stream(input).GetAsyncEnumerator();
        }
		catch
		{}
        
        while (true)
        {
            try
            {
                if (!await enumerator.MoveNextAsync())
                {
                    yield break;
                }
            }
            catch (System.Exception e)
            {
				LoggerManager.LogDebug("Exception thrown during stream", "", "e", e);
				Chainable.Error = e;

				if (HandleThrownException(e) != null)
					throw;

				enumerator = StreamWithFallback(input).GetAsyncEnumerator();
				continue;
            }
            
            yield return enumerator.Current;
        }

        // old implementation which relied on the stream returning an exception
        // object
		// await foreach (var output in base.Stream(input))
		// {
		// 	if (output is Exception e)
		// 	{
		// 		HandleThrownException(e);
        //
		// 		// throw error when the fallback stack is empty
		// 		if (FallbackStack.Count == 0)
		// 		{
		// 			throw e;
		// 		}
        //
		// 		await foreach (var outputFallback in StreamWithFallback(input))
		// 		{
		// 			yield return outputFallback;
		// 		}
		// 	}
		// 	else
		// 	{
		// 		yield return output;
		// 	}
		// }
	}


	public override Exception? HandleThrownException(Exception e)
	{
		// check if we are either out of fallbacls, or this exception type isn't
		// handled by the fallbacks
		if (!SetTargetFallbackChainable() || !ExceptionTypeHandledByFallbacks(e.GetType()))
		{
			return base.HandleThrownException(e);
		}
		else
		{
			if (Input is Dictionary<string, object> d)
			{
				d.Merge(new Dictionary<string, object>() {{ ExceptionKey, e}});
			}

			return null;
		}
	}

	public bool ExceptionTypeHandledByFallbacks(Type t)
	{
		bool handled = false;
		foreach (var exceptionType in FallbackExceptions)
		{
			if (t.IsSubclassOf(exceptionType) || t == exceptionType)
			{
				handled = true;

				LoggerManager.LogDebug("Exception type handled by fallback", "", "exceptionType", t.Name);
				break;
			}
		}

		return handled;
	}

	public bool SetTargetFallbackChainable()
	{
		LoggerManager.LogDebug("Chainable has failed, falling back");

		// make sure we're running with some configured fallbacks
		if (Fallbacks.Count == 0)
		{
			throw new ArgumentOutOfRangeException("There are no configured fallback chainables");
		}

		if (FallbackStack == null)
		{
			FallbackStack = new(Fallbacks.ToArray().Reverse());

			LoggerManager.LogDebug("Creating fallback stack", "", "fallbacks", FallbackStack.Count);
		}

		if (FallbackStack.TryPeek(out var fb))
		{
			Target = FallbackStack.Pop();

			LoggerManager.LogDebug("Falling back", "", "fallback", Target.GetType().Name);
			LoggerManager.LogDebug("Remaining fallbacks", "", "fallbacks", FallbackStack.Count);

			return true;
		}
		else
		{
			LoggerManager.LogDebug("No fallbacks remaining, chainable failed", "", "type", Chainable.GetType().Name);

			return false;
		}

	}
}
