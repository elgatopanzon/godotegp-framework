namespace GodotEGP.Service;

using Godot;
using System;
using System.Collections.Generic;

using GodotEGP.Random;
using GodotEGP.Logging;

public partial class RandomService : Service
{
	private Dictionary<string, NumberGenerator> _randomInstances = new Dictionary<string, NumberGenerator>();

	public override void _Ready()
	{
		_SetServiceReady(true);
	}

	public NumberGenerator Get(string instanceName = "default")
	{
		if (!_randomInstances.TryGetValue(instanceName, out NumberGenerator randomInstance))
		{
			// instead of throwing an error, we just create a basic instance
			randomInstance = _CreateRandomInstance();

			RegisterInstance(randomInstance, instanceName);
		}

		return randomInstance;
	}

	private NumberGenerator _CreateRandomInstance(ulong seed = 0, ulong state = 0)
	{
		return new NumberGenerator(seed, state);
	}

	public NumberGenerator RegisterInstance(NumberGenerator randomInstance, string instanceName)
	{
		if (!_randomInstances.TryAdd(instanceName, randomInstance))
		{
			throw new RandomInstanceExistsException($"Instance with name {instanceName} already exists");		
		}

		LoggerManager.LogDebug("Registering new instance", "", "instance", instanceName);

		return randomInstance;
	}
}

public partial class RandomInstanceExistsException : Exception
{
	public RandomInstanceExistsException(string message) : base(message)
	{
		
	}
}
