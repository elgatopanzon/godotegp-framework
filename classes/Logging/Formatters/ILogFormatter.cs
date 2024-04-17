namespace GodotEGP.Logging.Formatters;

using Godot;
using System;

public partial interface ILogFormatter
{
	public object Format(Logging.Message loggerMessage);
}
