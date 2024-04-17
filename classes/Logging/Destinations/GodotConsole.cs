namespace GodotEGP.Logging.Destinations;

using Godot;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using GodotEGP.Logging.Formatters;

public partial class GodotConsole : ILoggingDestination
{
	public bool Enabled { get; set; }

	private ILogFormatter _loggerFormatter;

	public GodotConsole(ILogFormatter loggerFormatter = null)
	{
		if (loggerFormatter == null)
		{
			loggerFormatter = new GodotRichFormatter();
		}
		_loggerFormatter = loggerFormatter;
		Enabled = true; // enabled by default
	}

	public bool Process(Logging.Message loggerMessage)
	{
		if (Enabled)
		{
			// var jsonString = JsonConvert.SerializeObject(
        	// loggerMessage, Formatting.Indented,
        	// new JsonConverter[] {new StringEnumConverter()});
            //
        	// GD.Print(jsonString);
        	GD.PrintRich(_loggerFormatter.Format(loggerMessage));
        	return true;
		}

		return false;
	}
}
