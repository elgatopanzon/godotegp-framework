/**
 * @author      : ElGatoPanzon (contact@elgatopanzon.io) Copyright (c) ElGatoPanzon
 * @file        : DefaultConsole
 * @created     : Sunday Dec 31, 2023 00:02:30 CST
 */

namespace GodotEGP.Logging.Destination;

using Godot;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using GodotEGP.Logging.Formatter;

public partial class DefaultConsole : IDestination
{
	public bool Enabled { get; set; }

	private IFormatter _loggerFormatter;

	public DefaultConsole(IFormatter loggerFormatter = null)
	{
		if (loggerFormatter == null)
		{
			loggerFormatter = new TextFormatter();
		}
		_loggerFormatter = loggerFormatter;
		Enabled = true; // enabled by default
	}

	public bool Process(Logging.Message loggerMessage)
	{
		if (Enabled)
		{
        	Console.WriteLine(_loggerFormatter.Format(loggerMessage));
        	return true;
		}

		return false;
	}
}
