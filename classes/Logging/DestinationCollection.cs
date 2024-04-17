namespace GodotEGP.Logging;

using Godot;
using System;
using System.Collections.Generic;

using GodotEGP.Logging.Destinations;

public partial class DestinationCollection
{
	private List<ILoggingDestination> _loggerDestinations = new List<ILoggingDestination>();

	public void AddDestination(ILoggingDestination destination)
	{
		_loggerDestinations.Add(destination);
	}

	public bool RemoveDestination(ILoggingDestination destination)
	{
		return _loggerDestinations.Remove(destination);
	}

	public List<ILoggingDestination> GetDestinations()
	{
		return _loggerDestinations;
	}
}
