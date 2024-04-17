/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableParallel
 * @created     : Wednesday Apr 03, 2024 14:33:07 CST
 */

namespace GodotEGP.Chainables;

using Godot;
using GodotEGP.Objects.Extensions;
using GodotEGP.Objects.Validated;
using GodotEGP.Logging;
using GodotEGP.Service;
using GodotEGP.Event.Events;
using GodotEGP.Config;

using System.Collections.Generic;
using System.Threading.Tasks;

public partial class ChainableParallel : ChainablePassthrough
{
	public Dictionary<string, IChainable> ParallelStack { get; set; } = new();
	public Dictionary<string, object> ParallelOutput { get; set; } = new();
	public List<IChainable> Processing { get; set; } = new();
	public List<IChainable> Finished { get; set; } = new();
	TaskCompletionSource<Dictionary<string, object>> _tcs = new();

	private readonly object _executeLock = new object();
	private readonly object _executeFinishedLock = new object();

	public ChainableParallel(Dictionary<string, IChainable> parallelStack = null)
	{
		if (parallelStack != null)
		{
			ParallelStack = parallelStack;
		}
	}

	public async override Task<object> _Process()
	{
		// create the task completion source to hold the processed output

		LoggerManager.LogDebug("Executing parallel stack", "", "parallelKeys", ParallelStack.Keys);
		LoggerManager.LogDebug("Parallel max concurrency", "", "maxConcurrency", Config.MaxConcurrency);

		this.Subscribe<EventChainableFinished>(_On_ChainableEventFinished, isHighPriority:true);
		this.Subscribe<EventChainableError>(_On_ChainableEventError, isHighPriority:true);

		// execute the parallel scheduler
		ExecuteParallelStack();

		// return the tcs Task with the assigned output
		return await _tcs.Task;
	}

	// process each of the parallel items one by one until they are all done
	public void ExecuteParallelStack()
	{
		lock(_executeLock)
		{
			if (Finished.Count < ParallelStack.Count)
			{
				foreach (var parallel in ParallelStack)
				{
					if (Processing.Contains(parallel.Value) || Finished.Contains(parallel.Value))
					{
						continue;
					}

					LoggerManager.LogDebug("Queuing for parallel execution", "", "parallelKey", parallel.Key);

					// TODO: figure out why this doesn't work and we get events
					// for random other objects??
					// parallel.Value.SubscribeOwner<EventChainableFinished>(_On_ChainableEventFinished, isHighPriority:true, oneshot:true);
					// parallel.Value.SubscribeOwner<EventChainableError>(_On_ChainableEventError, isHighPriority:true, oneshot:true);

					parallel.Value.Config.RunName = $"parallel-{parallel.Key}";

					Processing.Add(parallel.Value);
					parallel.Value.Run(Input);

					if (Config.MaxConcurrency > 0 && Processing.Count >= Config.MaxConcurrency)
					{
						LoggerManager.LogDebug("Parallel stack reached max concurrency", "", "max", Config.MaxConcurrency);
						break;
					}
				}
			}

			// if finished matches the original count, then we've processed all of
			// them and we can assign the outputs
			else
			{
				LoggerManager.LogDebug("Parallel stack finished executing", "", "parallelKeys", ParallelStack.Keys);

				foreach (var parallel in ParallelOutput)
				{
					LoggerManager.LogDebug("Parallel output result", "", parallel.Key, parallel.Value);
				}

				_tcs.TrySetResult(ParallelOutput);
			}

			if (Error != null)
			{
				HandleThrownException(Error);
				throw Error;
			}
		}
	}

	public void _On_ChainableEventFinished(EventChainableFinished e)
	{
		// LoggerManager.LogDebug("Parallel stack item finished", e.Chainable.Config.RunName, "output", $"output:{e.Chainable.Output}, hash:{e.Chainable.GetHashCode()}, stackHashes:{String.Join(",", ParallelStack.Select(x => x.Value.GetHashCode()))}");

		lock (_executeFinishedLock)
		{
			foreach (var parallel in ParallelStack)
			{
				// TODO: figure out why we're getting multiple finished events for
				// the same chainable object
				if (parallel.Value == e.Chainable && !Finished.Contains(e.Chainable))
				{
					LoggerManager.LogDebug("Marking parallel run as finished", "", parallel.Key, e.Chainable.Output);
					
					ParallelOutput.Add(parallel.Key, e.Chainable.Output);

					Processing.Remove(e.Chainable);
					Finished.Add(e.Chainable);

					ExecuteParallelStack();
				}
			}

		}

	}
	public void _On_ChainableEventError(EventChainableError e)
	{
		Error = e.Error;

		LoggerManager.LogDebug("Parallel stack item error", "", "error", Error);

		lock (_executeFinishedLock)
		{
			foreach (var parallel in ParallelStack)
			{
				// TODO: figure out why we're getting multiple finished events for
				// the same chainable object
				if (parallel.Value == e.Chainable && !Finished.Contains(e.Chainable))
				{
					ExecuteParallelStack();
				}
			}
		}
	}

	// original version, is flawed because it has to process in batches the max
	// size of max concurrency with all of them finishing before the next batch
	// begins, which is inefficient
	// public async override Task<object> _Process()
	// {
	// 	LoggerManager.LogDebug("Executing parallel stack", "", "parallelKeys", ParallelStack.Keys);
	// 	LoggerManager.LogDebug("Parallel max concurrency", "", "maxConcurrency", Config.MaxConcurrency);
    //
	// 	List<string> runTasksProcessed = new();
    //
	// 	while (runTasksProcessed.Count != ParallelStack.Count)
	// 	{
	// 		List<Task> runTasks = new();
	// 		int processedCountTemp = 0;
    //
	// 		foreach (var parallel in ParallelStack)
	// 		{
	// 			if (runTasksProcessed.Contains(parallel.Key))
	// 			{
	// 				continue;
	// 			}
    //
	// 			LoggerManager.LogDebug("Queuing for parallel execution", "", "parallelKey", parallel.Key);
    //
	// 			runTasks.Add(parallel.Value.Run(Input));
	// 			runTasksProcessed.Add(parallel.Key);
    //
	// 			processedCountTemp++;
    //
	// 			if (Config.MaxConcurrency > 0 && processedCountTemp == Config.MaxConcurrency)
	// 			{
	// 				LoggerManager.LogDebug("Parallel stack reached max concurrency", "", "max", Config.MaxConcurrency);
	// 				break;
	// 			}
	// 		}
    //
	// 		// run and wait for all the chainables to finish
	// 		await Task.WhenAll(runTasks);
	// 	}
    //
	// 	LoggerManager.LogDebug("Parallel stack finished executing", "", "parallelKeys", ParallelStack.Keys);
    //
	// 	// now assign outputs to dictionary by key
	// 	Dictionary<string, object> parallelOutputs = new();
    //
	// 	foreach (var parallel in ParallelStack)
	// 	{
	// 		parallelOutputs[parallel.Key] = parallel.Value.Output;
    //
	// 		LoggerManager.LogDebug("Parallel output result", "", parallel.Key, parallel.Value.Output);
	// 	}
    //
	// 	return parallelOutputs;
	// }
}
