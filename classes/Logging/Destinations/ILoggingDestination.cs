namespace GodotEGP.Logging.Destinations;

using Godot;
using System;

public partial interface ILoggingDestination
{
	bool Enabled { get; set; }
	public bool Process(Logging.Message loggerMessage);
}
