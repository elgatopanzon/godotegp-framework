namespace GodotEGP.Logging.Destination;

using Godot;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using GodotEGP.Logging.Formatter;

public partial class GodotConsole : IDestination
{
	public bool Enabled { get; set; }

	private IFormatter _loggerFormatter;

	public GodotConsole(IFormatter loggerFormatter = null)
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
