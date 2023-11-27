namespace GodotEGP.Logging;

using Godot;
using System;
using System.Collections.Generic;

using GodotEGP.Logging.Destination;

public partial class DestinationCollection
{
	private List<IDestination> _loggerDestinations = new List<IDestination>();

	public void AddDestination(IDestination destination)
	{
		_loggerDestinations.Add(destination);
	}

	public bool RemoveDestination(IDestination destination)
	{
		return _loggerDestinations.Remove(destination);
	}

	public List<IDestination> GetDestinations()
	{
		return _loggerDestinations;
	}
}
