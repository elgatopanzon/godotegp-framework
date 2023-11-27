namespace GodotEGP.Logging.Formatter;

using Godot;
using System;

public partial interface IFormatter
{
	public object Format(Logging.Message loggerMessage);
}
