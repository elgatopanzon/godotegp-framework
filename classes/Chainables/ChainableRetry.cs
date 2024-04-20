/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : ChainableRetry
 * @created     : Saturday Apr 06, 2024 22:45:42 CST
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

public partial class ChainableRetry : ChainableFallback
{
	// re-create the list of Fallbacks when accessing Retries value
	private int _retries = 0;
	public int MaxRetries { 
		get {
			return _retries;
		}
		set {
			_retries = value;
		}
	}

	// passthrough to FallbackExceptions since it's already implemented
	public List<Type> RetryExceptions { 
		get {
			return FallbackExceptions;
		}
		set {
			FallbackExceptions = value;
		}
	}

	// how long to wait before retrying
	public int WaitTimeSec { get; set; } = 1;
	public int RetriesRemaining { 
		get {
			if (FallbackStack == null)
			{
				return MaxRetries;
			}
			else
			{
				return FallbackStack.Count;
			}
		}
	}

	public int WaitTime {
		get {
			return (WaitTimeSec * (MaxRetries - RetriesRemaining));
		}
	}


	/************************
	*  Object pool methods  *
	************************/

	public override void Reset()
	{
		_retries = 0;
		WaitTimeSec = 1;

		base.Reset();
	}

	public override void Init(params object[] p)
	{
		InitChainable((p != null && p.Length >= 1) ? p[0] : null);
	}
	
	public void InitChainable()
	{

		base.InitChainable();
	}

	public void CreateFallbacksForRetry()
	{
		if (Target != null && (Fallbacks == null || Fallbacks.Count == 0))
		{
			Fallbacks = new();
			for (int i = 0; i < (_retries); i++)
			{
				Fallbacks.Add(Target);
			}
		}
	}

	public override Exception? HandleThrownException(Exception e)
	{
		CreateFallbacksForRetry();

		int waitTime = WaitTime;
		LoggerManager.LogDebug("Waiting before trying again", "", "waitTime", waitTime);
		LoggerManager.LogDebug("Retries remaining", "", "retriesRemaining", RetriesRemaining);

		if (waitTime > 0)
		{
			Task.WaitAll(Task.Run(async () => await Task.Delay(waitTime * 1000)));
		}

		return base.HandleThrownException(e);
	}

	public override void _HookPreRun()
	{
	}
}
