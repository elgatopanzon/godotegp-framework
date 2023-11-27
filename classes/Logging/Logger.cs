namespace GodotEGP.Logging;

using Godot;
using System;

using GodotEGP.Logging.Destination;

public partial class Logger
{
	public Logging.Message.LogLevel LogLevel = Logging.Message.LogLevel.Debug; // debug by default

	public DestinationCollection LoggerDestinationCollection { set; get; }

	public Logger(DestinationCollection loggerDestinationCollection)
	{
		LoggerDestinationCollection = loggerDestinationCollection;
	}

	public void ProcessLoggerMessage(Logging.Message loggerMessage)
	{
        foreach (IDestination loggerDestination in LoggerDestinationCollection.GetDestinations())
        {
        	loggerDestination.Process(loggerMessage);
        }
	}
}
